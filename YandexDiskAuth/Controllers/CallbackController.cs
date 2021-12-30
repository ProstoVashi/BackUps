using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YandexDiskAuth.Services;

namespace YandexDiskAuth.Controllers {
    [ApiController]
    [Route("yd_auth/[controller]")]
    public class CallbackController : ControllerBase {
        private static readonly string[] Summaries = new[] {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CallbackController> _logger;
        private readonly AuthYandexService _authYandexService;

        public CallbackController(ILogger<CallbackController> logger, AuthYandexService authYandexService) {
            _logger = logger;
            _authYandexService = authYandexService;
        }

        [HttpGet("approve_code")]
        public IActionResult HandleAuthCallback([FromQuery] string code) {
            return Ok();
        }

        [HttpGet]
        public IActionResult Ping() {
            return Ok("I'm alive!");
        }
        
        // [HttpGet]
        // public IEnumerable<WeatherForecast> Get() {
        //     var rng = new Random();
        //
        //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
        //                          Date = DateTime.Now.AddDays(index),
        //                          TemperatureC = rng.Next(-20, 55),
        //                          Summary = Summaries[rng.Next(Summaries.Length)]
        //                      })
        //                      .ToArray();
        // }
    }
}