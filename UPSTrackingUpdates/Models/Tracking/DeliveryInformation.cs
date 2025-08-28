using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class DeliveryInformation
    {
        [JsonProperty("deliveryPhoto")]
        public DeliveryPhoto DeliveryPhoto { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("receivedBy")]
        public string ReceivedBy { get; set; }

        [JsonProperty("signature")]
        public DeliverySignature Signature { get; set; }

        [JsonProperty("pod")]
        public ProofOfDelivery ProofOfDelivery { get; set; }

        public string UPSTrackingNumber { get; set; }

        public string ProofOfDeliveryPDFPath { get; set; }

        public bool DeliverySignatureIncluded { get; set; }

        public bool DeliveryPhotographIncluded { get; set; }
    }
}
