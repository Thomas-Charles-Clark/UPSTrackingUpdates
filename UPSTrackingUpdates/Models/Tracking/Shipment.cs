using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Shipment
    {
        [JsonProperty("inquiryNumber")]
        public string InquiryNumber { get; set; }

        [JsonProperty("package")]
        public IEnumerable<Package> Packages { get; set; }

        [JsonProperty("userRelation")]
        public IEnumerable<string> UserRelation { get; set; }

        [JsonProperty("warnings")]
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
