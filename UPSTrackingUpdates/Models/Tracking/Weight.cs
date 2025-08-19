using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Weight
    {
        [JsonProperty("unitOfMeasurement")]
        public string UnitOfMeasurement { get; set; }

        [JsonProperty("weight")]
        public string Value { get; set; }
    }
}
