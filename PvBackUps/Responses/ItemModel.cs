using System.Text.Json.Serialization;
using PvBackUps.Responses.Interfaces;

namespace PvBackUps.Responses {
    public class ItemModel : INameModel {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}