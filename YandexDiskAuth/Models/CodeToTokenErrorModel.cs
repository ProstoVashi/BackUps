using System.Text.Json.Serialization;

namespace YandexDiskAuth.Models {
    public class CodeToTokenErrorModel {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        
        [JsonPropertyName("error_description")]
        public string Description { get; set; }
    }
}