using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class PaymentInformation
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string CurrencyCode { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("paid")]
        public string Paid { get; set; }

        [JsonProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
