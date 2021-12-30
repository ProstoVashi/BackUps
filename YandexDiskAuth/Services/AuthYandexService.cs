using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using Tools.Utils;
using YandexDiskAuth.Configs;
using YandexDiskAuth.Models;

namespace YandexDiskAuth.Services {
    public class AuthYandexService {
        private const string BASE_AUTH_URL = "https://oauth.yandex.ru";
        private static readonly TimeSpan AutoCloseAuthProcess = TimeSpan.FromMinutes(5);
        
        private readonly ILogger<AuthYandexService> _logger;
        private readonly YandexAppSettings _yandexAppSettings;

        public AuthYandexService(ILogger<AuthYandexService> logger, IOptions<YandexAppSettings> option) {
            _logger = logger;
            
            _yandexAppSettings = option.Value;
        }

        /// <summary>
        /// Запрашивает код подтвержения. Перенаправляет в бразуер для подтверждения действий
        /// </summary>
        /// <returns>Возвращает результат, был ли запрос удачным</returns>
        internal async Task<bool> RequestCodeAsync() {
            var sb = new StringBuilder();
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
            if (process != null) {
                var task = process.WaitForExitAsync(cancelTokenSource.Token);
                await task;

                return task.IsCompletedSuccessfully;
            } else {
                return false;
            }
        }

        internal async Task<(bool Success, CodeToTokenModel TokenInfo, CodeToTokenErrorModel Error)> ExchangeCodeOnToken(string code) {
            IRestClient authClient = new RestClient(BASE_AUTH_URL);
            RestRequest request = new RestRequest("token", Method.POST);
            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_yandexAppSettings.ClientId}:{_yandexAppSettings.Secret}"));
            var parameterString = $"grant_type=authorization_code&code={code}";
            request.AddHeader("Authorization", $"Basic {authString}")
                   .AddOrUpdateHeader("content-type", "application/x-www-form-urlencoded")
                   .AddParameter("application/x-www-form-urlencoded", parameterString, ParameterType.RequestBody);

            var response = await authClient.ExecuteAsync(request);
            if (response.IsSuccessful) {
                return (true, JsonSerializer.Deserialize<CodeToTokenModel>(response.Content), null);
            } else {
                return (true, null, JsonSerializer.Deserialize<CodeToTokenErrorModel>(response.Content));
            }
        }
    }
}