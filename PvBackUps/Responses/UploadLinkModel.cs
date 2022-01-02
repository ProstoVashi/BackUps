using System.Text.Json.Serialization;

namespace PvBackUps.Responses {
    public class UploadLinkModel {
        [JsonPropertyName("operation_id")]
        public string OperationId { get; set; }
        /// <summary>
        /// URL. Может быть шаблонизирован, см. ключ templated.
        /// </summary>
        [JsonPropertyName("href")]
        public string Href { get; set; }

        /// <summary>
        /// HTTP-метод для запроса URL из ключа href.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// Признак URL, который был шаблонизирован согласно RFC 6570. Возможные значения:
        /// «true» — URL шаблонизирован: прежде чем отправлять запрос на этот адрес, следует указать нужные значения параметров вместо значений в фигурных скобках.
        /// «false» — URL может быть запрошен без изменений.
        /// </summary>
        [JsonPropertyName("templated")]
        public bool Templated { get; set; }
    }
}