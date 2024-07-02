
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HngStageOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HelloController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private class HelloModel
        {
            public string client_ip { get; set; }
            public string location { get; set; }
            public string greeting { get; set; }
                         
  
        };
        private class WeatherResponse
        {
            [JsonPropertyName("current")]
            public Current current { get; set; }

        }
        private class LocationResponse
        {
            [JsonPropertyName("city")]
            public string city { get; set; }

        }
        private class Current
        {
            [JsonPropertyName("temp_c")]
            public double temp_c { get; set; }

        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string visitor_name)
        {
            if (string.IsNullOrEmpty(visitor_name))
            {
                throw new Exception("Visitor name is required.");
            }


            string _ipAddress = string.Empty;
            string _location = string.Empty;
            double _temperature;
            try
            {
                _ipAddress = GetClientIpAddress(HttpContext);
                var ipAddressWithoutPort = _ipAddress?.Split(':')[0];
                _location = await GetLocation(ipAddressWithoutPort);
                _temperature = await GetTemperatureAsync(ipAddressWithoutPort);
            }
            catch (Exception ex)
            {
                throw new Exception($"unable to retrieve location from ip {_ipAddress} supplied, with" +
                    $"error message {ex.Message} ");
            }
            var response = new HelloModel
            {
                client_ip = _ipAddress,
                location = _location,
                greeting = $"Hello, {visitor_name}!, the temperature is {_temperature} degrees Celcius in  {_location}",

            };

            return Ok(response);
        }
        private string GetClientIpAddress(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();

            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            // Handle IPv4 and IPv6 loopback addresses
            if (ip == "::1" || ip == "127.0.0.1" || ip.StartsWith("::ffff:"))
            {
                ip = "102.89.23.181"; 
            }

            return ip;
        }

        private string token = "900c452ab7bb0c";
        private async Task<string> GetLocation(string ipAddress)
        {

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ipinfo.io/{ipAddress}/json?token={token}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LocationResponse>(json);
            return result.city;

        }

        private string wearherapiToken = "48a09036c1f949b294e175417240107";
        private async Task<double> GetTemperatureAsync(string location)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.weatherapi.com/v1/current.json?key={wearherapiToken}&q={location}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(content);

            return weatherData.current.temp_c;
        }
    }


}