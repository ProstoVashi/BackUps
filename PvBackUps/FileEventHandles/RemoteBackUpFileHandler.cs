using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles.Interfaces;
using PvBackUps.Responses;
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
            _logger.LogInformation("Try connect to url: '{Url}'", _tokenReceiveUrl);
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
            
            if (!await CheckAndRestoreSavePath(client)) {
                return;
            }

            if (await UploadFile(client, token, fileName, fullFileName)) {
                _logger.LogInformation("File '{File}' successfully uploaded", fullFileName);
            } else {
                _logger.LogWarning("File '{File}' wasn't uploaded", fullFileName);
            }
        }

        /// <summary>
        /// Returns TOKEN from custom YandexDiskAuth utility
        /// </summary>
        private async Task<string> GetTokenAsync() {
            var client = new RestClient(_tokenReceiveUrl);
            var response = await client.ExecuteAsync<string>(new RestRequest(Method.GET));
            return response.Data;
        }

        /// <summary>
        /// Check if path exists and try to create, if not.
        /// </summary>
        /// <returns>TRUE if path exists or was created, otherwise - FALSE</returns>
        private async Task<bool> CheckAndRestoreSavePath(IRestClient client) {
            var sb = new StringBuilder();
            for (int i = 0; i < _yandexDiskSubPaths.Length; i++) {
                sb.Append(_yandexDiskSubPaths[i]);
                var path = sb.ToString();

                var request = new RestRequest("resources", Method.GET)
                              .AddQueryParameter("fields", "_embedded,name")
                              .AddQueryParameter("path", path);

                var response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful) {
                    if (HandleCheckPathError(response)) {
                        return await CreateFolder(client, sb, i);
                    } else {
                        return false;
                    }   
                }
            }
            return true;
        }

        /// <summary>
        /// Goes through remain sub-paths and create YD-folders
        /// </summary>
        private async Task<bool> CreateFolder(IRestClient client,  StringBuilder sb, int index) {
            var path = sb.ToString();
            var request = new RestRequest("resources", Method.PUT)
                .AddQueryParameter("path", path);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful && !HandleCreateFolderError(response)) {
                return false;
            }
            
            if (++index == _yandexDiskSubPaths.Length) {
                return true;
            }

            sb.Append(_yandexDiskSubPaths[index]);
            return await CreateFolder(client, sb, index);
        }

        /// <summary>
        /// Upload file
        /// </summary>
        private async Task<bool> UploadFile(IRestClient client, string token, string fileName, string fullFileName) {
            var remoteFullFileName = $"{string.Join("", _yandexDiskSubPaths)}{fileName}";
            var linkRequest = new RestRequest("resources/upload", Method.GET)
                              .AddQueryParameter("path", remoteFullFileName)
                              .AddQueryParameter("overwrite", "false");
            var linkResponse = await client.ExecuteAsync(linkRequest);
            if (!linkResponse.IsSuccessful) {
                HandleUploadLinkError(linkResponse);
                return false;
            }

            if (!TryParseUploadLinkResponse(linkResponse, out var link)) {
                return false;
            }
            
            var url = new Uri(link.Href);
            var method = new HttpMethod(link.Method);
            await using var fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read);
            using var content = new StreamContent(fs);
            using var requestMessage = new HttpRequestMessage(method, url) { Content = content };
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);
            httpClient.Timeout = TimeSpan.FromHours(6);
            var uploadResponse = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (!uploadResponse.IsSuccessStatusCode) {
                HandleUploadFileError(uploadResponse);
            }

            return uploadResponse.IsSuccessStatusCode;
        }
        
        /// <summary>
        /// Validate check path exists response error code: if NotFound - it's ok, go create
        /// </summary>
        private bool HandleCheckPathError(IRestResponse response) {
            if (response.StatusCode == HttpStatusCode.NotFound) {
                return true;
            }
            _logger.LogError("Check path exists failed with unexpected error code ['{StringCode}':{Code}]: {Message}",
                             response.StatusCode.ToString(), response.StatusCode, response.Content);
            return false;
        }

        /// <summary>
        /// Validate create folder response error code: if Conflict - it's ok, it's already exists, skip it
        /// </summary>
        private bool HandleCreateFolderError(IRestResponse response) {
            if (response.StatusCode == HttpStatusCode.Conflict) {
                _logger.LogWarning("Try create path but it's already existed: {Message}", response.Content);
                return true;
            }
            
            _logger.LogError("Create folder failed with unexpected error code ['{StringCode}':{Code}]: {Message}",
                             response.StatusCode.ToString(), response.StatusCode, response.Content);
            return false;
        }

        /// <summary>
        /// Try handle expected errors and log received one
        /// </summary>
        private void HandleUploadLinkError(IRestResponse response) {
            switch (response.StatusCode) {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.InsufficientStorage: {
                    _logger.LogWarning("Get upload link failed, as there is no enough space. ['{StringCode}':{Code}]: {Message}", 
                                       response.StatusCode.ToString(), response.StatusCode, response.Content);
                    return;
                }
                case HttpStatusCode.Conflict:
                case HttpStatusCode.Locked:
                case HttpStatusCode.TooManyRequests: {
                    _logger.LogWarning("Get upload link failed, as it's already been in process or exists. ['{StringCode}':{Code}]: {Message}",
                                       response.StatusCode.ToString(), response.StatusCode, response.Content);
                    return;
                }
                default: {
                    _logger.LogError("Get upload link failed with unexpected error code ['{StringCode}':{Code}]: {Message}",
                                     response.StatusCode.ToString(), response.StatusCode, response.Content);
                    return;
                }
            }
        }

        /// <summary>
        /// Validate UploadLink content and returns it if valid
        /// </summary>
        private bool TryParseUploadLinkResponse(IRestResponse response, out UploadLinkModel link) {
            link = JsonSerializer.Deserialize<UploadLinkModel>(response.Content);
            if (link == null || string.IsNullOrEmpty(link.Href) || string.IsNullOrEmpty(link.Method)) {
                _logger.LogError("Can't parse UploadLink response content or content is invalid: {Message}", response.Content);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Log upload file result
        /// </summary>
        private void HandleUploadFileError(HttpResponseMessage response) {
            _logger.LogError("Upload file failed with error code ['{StringCode}':{Code}]: {Message}",
                             response.StatusCode.ToString(), response.StatusCode, response.Content);
        }
    }
    
}