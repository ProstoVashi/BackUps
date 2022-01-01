using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles.Interfaces;
using RestSharp;
using Tools.Utils;

namespace PvBackUps.FileEventHandles {
    public class RemoteBackUpFileHandler : IFileEventHandler {
        private const string YD_BASE_URL = "https://cloud-api.yandex.net/v1/disk";

        private readonly ILogger<RemoteBackUpFileHandler> _logger;

        private readonly string _tokenReceiveUrl;
        private readonly string[] _yandexDiskSubPaths;
        
        public RemoteBackUpFileHandler(ILogger<RemoteBackUpFileHandler> logger, IOptions<RemoteStorageSettings> options) {
            _logger = logger;
            var settings = options.Value;
            _tokenReceiveUrl = settings.TokenReceiveUrl;
            _yandexDiskSubPaths = settings.YandexDiskSubPaths;
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            _logger.LogInformation("Remote storage handler started with file: {File}", e.FullPath);
            TrySaveToStorage(e.Name, e.FullPath);
        }

        private async void TrySaveToStorage(string fileName, string fullFileName) {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) {
                _logger.LogError("Can't receive token to process remote save operation for file: '{File}'", fullFileName);
                return;
            }

            var client = RestSettings.GetClient(YD_BASE_URL)
                                     .AddDefaultHeader("Authorization", $"OAuth {token}");
            
            //ToDo: check paths and create on null
            await CheckAndRestoreSavePath();
        }

        /// <summary>
        /// Returns TOKEN from custom YandexDiskAuth utility
        /// </summary>
        private async Task<string> GetTokenAsync() {
            var client = new RestClient(_tokenReceiveUrl);
            var response = await client.ExecuteAsync<string>(new RestRequest(Method.GET));
            return response.Data;
        }

        private async Task CheckAndRestoreSavePath() {
            
        }
    }
}