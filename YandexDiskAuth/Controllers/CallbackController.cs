using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YandexDiskAuth.Services;

namespace YandexDiskAuth.Controllers {
    [ApiController]
    [Route("06fb0f8630ac75b2515cc8d0862140d2172ad9b336886388c141e88ffe94fa6byd_auth/[controller]")]
    public class CallbackController : ControllerBase {
        private readonly AuthYandexService _authYandexService;
        private readonly TokenHandlerService _tokenHandlerService;

        public CallbackController(AuthYandexService authYandexService, TokenHandlerService tokenHandlerService) {
            _authYandexService = authYandexService;
            _tokenHandlerService = tokenHandlerService;
        }

        /// <summary>
        /// Callback url for receive auth-code, that can be changed on token-info
        /// </summary>
        [HttpGet("approve_code")]
        public async Task<IActionResult> HandleAuthCallback([FromQuery] string code) {
            var result = await _authYandexService.ExchangeCodeOnToken(code);
            if (result.Success) {
                var saveResult = _tokenHandlerService.WriteTokenInfo(result.TokenInfo);
                return Ok($"Token was received! But saved? {saveResult}");
            }
            return Ok("Token was NOT received");
        }

        [HttpGet]
        public IActionResult Ping() {
            return Ok("I'm alive!");
        }
    }
}