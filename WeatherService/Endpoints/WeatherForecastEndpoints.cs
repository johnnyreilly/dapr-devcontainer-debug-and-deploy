using Config;

using Microsoft.Extensions.Options;

using RestSharp;
using RestSharp.Authenticators;

using Dapr;

namespace WeatherService.Endpoints;

public static class WeatherForecastEndpoints
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public record SendWeatherForecastBody(string? Email);

    public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/SendWeatherForecast",
            // Dapr subscription in [Topic] routes weather-forecasts topic to this route
            [Topic("weather-forecast-pub-sub", "weather-forecasts")]
            async (
                SendWeatherForecastBody body,
                ILogger<SendWeatherForecastBody> logger,
                IOptions<MailConfig> options
            ) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(body.Email)) throw new Exception("Email required");

                    var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    })
                    .ToArray();

                    var toEmailAddress = body.Email;
                    var text = $@"The weather forecast is:

{string.Join("\n", weatherForecast.Select(wf => $"On {wf.Date} the weather will be {wf.Summary}"))}        
";

                    await SendSimpleMessage(
                        mailgunApiKey: options.Value.MailgunApiKey,
                        toEmailAddress: toEmailAddress,
                        text: text
                    );

                    return Results.Ok($"We have mailed {toEmailAddress} with the following:\n\n{text})");
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, $"Problem!");

                    return Results.BadRequest(exc.Message);
                }
            });

        return endpoints;
    }

    static async Task<RestResponse> SendSimpleMessage(string mailgunApiKey, string toEmailAddress, string text)
    {
        RestClient client = new(new RestClientOptions
        {
            BaseUrl = new Uri("https://api.mailgun.net/v3")
        })
        {
            Authenticator =
            new HttpBasicAuthenticator("api", mailgunApiKey)
        };
        RestRequest request = new();
        request.AddParameter("domain", "mg.priou.co.uk", ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", "John Reilly <johnny_reilly@hotmail.com>");
        request.AddParameter("to", toEmailAddress);
        request.AddParameter("subject", "Weather forecast");
        request.AddParameter("text", text);

        return await client.PostAsync(request);
    }
}
