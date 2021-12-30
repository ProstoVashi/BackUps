using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tools.Utils;
using YandexDiskAuth.Configs;
using YandexDiskAuth.Models;

namespace YandexDiskAuth.Services {
    public class TokenHandlerService {
        private static readonly TimeSpan AutoCloseReadTokenDelay = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan SleepTokenDelay = TimeSpan.FromSeconds(2);
        
        private readonly ILogger<TokenHandlerService> _logger;
        private readonly AuthYandexService _authYandexService;
        private readonly TokenStorageSettings _tokenStorageSettings;

        public TokenHandlerService(ILogger<TokenHandlerService> logger, IOptions<TokenStorageSettings> options, AuthYandexService authYandexService) {
            _logger = logger;
            _authYandexService = authYandexService;
            _tokenStorageSettings = options.Value;
        }

        /// <summary>
        /// Returns token value, written in storage. If there is no value in Registry, Auth.Request will be created and we will await in cycle when it will be completed
        /// </summary>
        /// <returns>Token, if info exists, and null in other way</returns>
        internal async Task<string> GetToken() {
            return await ExceptionHandler.SafetyExec(async () => {
                using var cancelTokenSource = new CancellationTokenSource(AutoCloseReadTokenDelay);
                var cancelToken = cancelTokenSource.Token;
            
                string jsonCodeToTokenModel;
                bool requested = false;
                do {
                    jsonCodeToTokenModel = RegisterHelper.GetValue(RegisterHelper.Root, _tokenStorageSettings.Location);
                    if (string.IsNullOrEmpty(jsonCodeToTokenModel)) {
                        if (!requested) {
                            requested = _authYandexService.RequestCodeAsync();
                        }
                        await Task.Delay(SleepTokenDelay, cancelToken);
                    }
                } while (string.IsNullOrEmpty(jsonCodeToTokenModel) && !cancelToken.IsCancellationRequested);
                
                if (string.IsNullOrEmpty(jsonCodeToTokenModel)) {
                    _logger.LogError("Can't find value in location: {Location}", _tokenStorageSettings.Location);
                    return null;
                }

                try {
                    var model = JsonSerializer.Deserialize<CodeToTokenModel>(jsonCodeToTokenModel, RestSettings.SerializerOptions);
                    if (model == null) {
                        _logger.LogError("Can't properly deserialize value: {Value}", jsonCodeToTokenModel);
                    }
                    return model?.Token;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Failed deserialize value: {Value}", jsonCodeToTokenModel);
                    return null;
                }
            }, _logger, false);
        }

        /// <summary>
        /// Write info to registry
        /// </summary>
        /// <param name="model">info with Token/Expire Time/Refresh Token</param>
        internal bool WriteTokenInfo(CodeToTokenModel model) {
            var jsonContent = JsonSerializer.Serialize(model, RestSettings.SerializerOptions);
            return RegisterHelper.SetValue(RegisterHelper.Root, _tokenStorageSettings.Location, jsonContent);
        }
    }
}