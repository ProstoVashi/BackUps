using System;
using System.Text.Json.Serialization;
using Tools.Extensions;

namespace YandexDiskAuth.Models {
    public class CodeToTokenModel {
        [JsonPropertyName("access_token")]
        public string Token { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonIgnore]
        public DateTime ExpireDate => ExpiresIn.MilliSecondsToDate();
    }
}