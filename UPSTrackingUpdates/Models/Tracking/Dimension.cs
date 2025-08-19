using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Dimension
    {
        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("unitOfDimension")]
        public string UnitOfDimension { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }
    }
}
