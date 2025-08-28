using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class DeliveryPhoto
    {
        [JsonProperty("isNonPostalCodeCountry")]
        public string IsNonPostalCodeCountry { get; set; }

        [JsonProperty("photo")]
        public object Photo { get; set; }

        [JsonProperty("photoCaptureInd")]
        public string PhotoCapturedIndicator { get; set; }

        [JsonProperty("photoDispositionCode")]
        public string PhotoDispositionCode { get; set; }
    }
}
