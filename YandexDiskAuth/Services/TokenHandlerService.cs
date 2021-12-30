using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YandexDiskAuth.Configs;
using YandexDiskAuth.Models;

namespace YandexDiskAuth.Services {
    public class TokenHandlerService {
        private readonly ILogger<TokenHandlerService> _logger;
        private readonly AuthYandexService _authYandexService;
        private readonly TokenStorageSettings _tokenStorageSettings;

        public TokenHandlerService(ILogger<TokenHandlerService> logger, IOptions<TokenStorageSettings> options, AuthYandexService authYandexService) {
            _logger = logger;
            _authYandexService = authYandexService;
            _tokenStorageSettings = options.Value;
        }

        internal Task<string> GetToken() {
            
        }
    }
}