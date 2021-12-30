using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using Tools.Utils;
using YandexDiskAuth.Configs;

namespace YandexDiskAuth.Services {
    public class AuthYandexService {
        private const string BASE_AUTH_URL = "oauth.yandex.ru";
        
        private readonly ILogger<AuthYandexService> _logger;
        private readonly IRestClient _authClient;
        private readonly YandexAppSettings _yandexAppSettings;

        public AuthYandexService(ILogger<AuthYandexService> logger, IOptions<YandexAppSettings> option) {
            _logger = logger;
            _authClient =  RestSettings.GetClient(BASE_AUTH_URL);
            _yandexAppSettings = option.Value;
        }


        /// <summary>
        /// Запрашивает код подтвержения
        /// </summary>
        /// <returns>Возвращает результат, был ли запрос удачным</returns>
        public async Task<bool> RequestCodeAsync() {
            var request = new RestRequest("authorize", Method.GET);
            request.AddQueryParameter("response_type", "code")
                   .AddQueryParameter("client_id", _yandexAppSettings.ClientId)
                   .AddQueryParameter("device_id", _yandexAppSettings.DeviceId);

            var requestCodeResponse = await ExceptionHandler.SafetyExec(() => _authClient.ExecuteAsync(request),
                                                                        _logger, false);

            return requestCodeResponse?.IsSuccessful ?? false;
        }
    }
}