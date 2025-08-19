using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class TrackResponse
    {
        [JsonProperty("shipment")]
        public IEnumerable<Shipment> Shipments { get; set; }
    }
}
