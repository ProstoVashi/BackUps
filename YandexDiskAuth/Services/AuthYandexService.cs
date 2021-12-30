using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using Tools.Utils;
using YandexDiskAuth.Configs;

namespace YandexDiskAuth.Services {
    public class AuthYandexService {
        private const string BASE_AUTH_URL = "oauth.yandex.ru";
        private static readonly TimeSpan AutoCloseAuthProcess = TimeSpan.FromMinutes(5);
        
        private readonly ILogger<AuthYandexService> _logger;
        private readonly IRestClient _authClient;
        private readonly YandexAppSettings _yandexAppSettings;

        public AuthYandexService(ILogger<AuthYandexService> logger, IOptions<YandexAppSettings> option) {
            _logger = logger;
            _authClient =  RestSettings.GetClient(BASE_AUTH_URL);
            _yandexAppSettings = option.Value;
        }


        /// <summary>
        /// Запрашивает код подтвержения. Перенаправляет в бразуер для подтверждения действий
        /// </summary>
        /// <returns>Возвращает результат, был ли запрос удачным</returns>
        public async Task<bool> RequestCodeAsync() {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.Append(BASE_AUTH_URL);
            sb.Append("/authorize?");

            void AddQueryParam(in string key, in string value, bool hasNext = true) {
                sb.Append(key);
                sb.Append('=');
                sb.Append(value);
                if (hasNext) {
                    sb.Append('&');
                }
            }
            AddQueryParam("response_type", "code");
            AddQueryParam("client_id", _yandexAppSettings.ClientId);
            AddQueryParam("device_id", _yandexAppSettings.DeviceId);

            using var cancelTokenSource = new CancellationTokenSource(AutoCloseAuthProcess);
            using var process = System.Diagnostics.Process.Start(sb.ToString());
            var task = process.WaitForExitAsync(cancelTokenSource.Token);
            await task;

            return task.IsCompletedSuccessfully;
        }
    }
}