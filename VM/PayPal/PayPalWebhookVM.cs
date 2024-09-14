using System.Text.Json.Serialization;

namespace Groove.VM.PayPal
{
    public class PayPalWebhookVM
    {
        public string Id { get; set; }
        [JsonPropertyName("event_type")]
        public string event_type { get; set; }
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("resource")]
        public Resource resource { get; set; }
        public List<LinkDescription> Links { get; set; }

        

        public class Resource
        {
            public string id { get; set; }
            public Amount Amount { get; set; }
            public string Status { get; set; }

            [JsonPropertyName("purchase_units")]
            public List<PurchaseUnit> purchase_units { get; set; }
        }

        public class Amount
        {
            public string CurrencyCode { get; set; }
            public string Value { get; set; }
        }

        public class LinkDescription
        {
            public string Href { get; set; }
            public string Rel { get; set; }
            public string Method { get; set; }
        }
        public class PurchaseUnit
        {
            [JsonPropertyName("custom_id")]
            public string custom_id { get; set; }
        }
    }

}
