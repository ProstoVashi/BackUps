using System.Text.Json;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace Tools.Utils {
    public static class RestSettings {
        public static readonly JsonSerializerOptions SerializerOptions = new() {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void UpdateJsonSerializerOptions(this JsonSerializerOptions opt) {
            opt.PropertyNameCaseInsensitive = true;
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        
        public static IRestClient GetClient(in string url) {
            var client = new RestClient(url);
            client.UseSystemTextJson(RestSettings.SerializerOptions);
            return client;
        }
    }
}