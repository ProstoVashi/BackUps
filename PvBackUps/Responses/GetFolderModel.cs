using System.Text.Json.Serialization;
using PvBackUps.Responses.Interfaces;

namespace PvBackUps.Responses {
    public class GetFolderModel : INameModel {
        [JsonPropertyName("name")]
        public string Name { get; set;  }

        [JsonPropertyName("_embedded")]
        public GetFolderModelInternal Internal { get; set; }
    }

    public class GetFolderModelInternal {
        [JsonPropertyName("items")]
        public ItemModel[] Items { get; set; }
    }
}