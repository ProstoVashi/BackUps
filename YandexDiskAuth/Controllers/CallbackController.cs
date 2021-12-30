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
        private readonly ILogger<CallbackController> _logger;
        private readonly AuthYandexService _authYandexService;

        public CallbackController(ILogger<CallbackController> logger, AuthYandexService authYandexService) {
            _logger = logger;
            _authYandexService = authYandexService;
        }

        [HttpGet("approve_code")]
        public async Task<IActionResult> HandleAuthCallback([FromQuery] string code) {
            var result = await _authYandexService.ExchangeCodeOnToken(code);
            
            return Ok($"Token was received? {result.Success}");
        }

        [HttpGet]
        public IActionResult Ping() {
            return Ok("I'm alive!");
        }
    }
}