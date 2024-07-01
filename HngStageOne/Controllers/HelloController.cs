
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

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
            public string Ip { get; set; }
            public string city { get; set; }
            public string greeting { get; set; }

        };
        [HttpGet]
        public  async Task<IActionResult> Get([FromQuery] string visitor_name)
        {
            
            var _ipAddress =  GeIpAddress(HttpContext);
            var _location = await GetLocation(_ipAddress);
          
            if (string.IsNullOrEmpty(visitor_name))
            {
                throw  new Exception("Visitor name is required.");
            }
            var response = new HelloModel
            {
                Ip = _ipAddress,
                city = _location,
                greeting = $"Hello, {visitor_name}!, the temperature is 11 degrees Celcius in  {_location}"

            };
          
            return Ok(response);
        }
        private string GeIpAddress(HttpContext context)

        {
            var ipAddress = context.Connection.RemoteIpAddress.ToString();

            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            }
            if (ipAddress == null)
            {
                return "ip is null";
            }
            string[] splitIPs = ipAddress.Split(", ");
            string localIP = splitIPs[0];
            return localIP;
        }
        private string token = "900c452ab7bb0c";
        private async Task<string> GetLocation(string ipAddress)
        {

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ipinfo.io/{ipAddress}/json?token={token}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<HelloModel>(json);
                return result.city;
            }
            else
            {
               
                return "Unknown";
            }
        }
    }
}
