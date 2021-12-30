using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YandexDiskAuth.Services;

namespace YandexDiskAuth.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ServiceRequestController : ControllerBase {
        private readonly TokenHandlerService _tokenHandlerService;

        public ServiceRequestController(TokenHandlerService tokenHandlerService) {
            _tokenHandlerService = tokenHandlerService;
        }

        /// <summary>
        /// Requesting token
        /// </summary>
        [HttpGet("token")]
        public async Task<string> GetTokenAsync() {
            var token = await _tokenHandlerService.GetToken();
            return string.IsNullOrEmpty(token) ? "" : token;
        }
        
        [HttpGet]
        public IActionResult Ping() {
            return Ok("I'm alive!");
        }
    }
}