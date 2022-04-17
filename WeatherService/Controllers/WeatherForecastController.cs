using Config;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using RestSharp;
using RestSharp.Authenticators;

namespace WeatherService.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly MailConfig _options;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IOptions<MailConfig> options
    )
    {
        _logger = logger;
        _options = options.Value;
    }
    
    public record SendWeatherForecastBody(string Email);

    [HttpPost(Name = "SendWeatherForecast")]
    public async Task<string> Post(SendWeatherForecastBody body)
    {
        try
        {
            if (string.IsNullOrEmpty(body.Email)) throw new Exception("Email required");

            var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = _options.Name + _options.Title + Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var toEmailAddress = body.Email;
            var text = $@"The weather forecast is:

{string.Join("\n", weatherForecast.Select(wf => $"On {wf.Date} the weather will be {wf.Summary}"))}        
";

            await SendSimpleMessage(
                toEmailAddress: toEmailAddress,
                text: text
            );

            return $"We have mailed {toEmailAddress} with the following:\n\n{text}";
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, $"Problem!");
            
            return exc.Message;
        }
    }

    async Task<RestResponse> SendSimpleMessage(string toEmailAddress, string text)
    {
        RestClient client = new(new RestClientOptions
        {
            BaseUrl = new Uri("https://api.mailgun.net/v3")
        })
        {
            Authenticator =
            new HttpBasicAuthenticator("api", _options.MailgunApiKey)
        };
        RestRequest request = new();
        request.AddParameter("domain", "mg.priou.co.uk", ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", "John Reilly <johnny_reilly@hotmail.com>");
        request.AddParameter("to", toEmailAddress);
        request.AddParameter("subject", "Hello");
        request.AddParameter("text", text);

        return await client.PostAsync(request);
    }
}
