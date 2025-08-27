using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class DeliverySignature
    {
        [JsonProperty("image")]
        public string Base64EncodedImage { get; set; }
    }
}
