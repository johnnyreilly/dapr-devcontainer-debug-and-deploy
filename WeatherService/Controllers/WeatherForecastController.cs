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

#pragma warning disable IDE0052
    private readonly ILogger<WeatherForecastController> _logger;
#pragma warning restore IDE0052
    private readonly MailConfig _options;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IOptions<MailConfig> options
    )
    {
        _logger = logger;
        _options = options.Value;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        await SendSimpleMessage();

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = _options.Name + _options.Title + Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    async Task<RestResponse> SendSimpleMessage()
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
        request.AddParameter("from", "John Reilly <johnnypaulreilly@gmail.com>");
        request.AddParameter("to", "johnny_reilly@hotmail.com");
        request.AddParameter("subject", "Hello");
        request.AddParameter("text", "Testing some Mailgun awesomeness!");

        return await client.PostAsync(request);
    }
}
