using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Milestone
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("current")]
        public string Current { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("linkedActivity")]
        public string LinkedActivity { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("subMilestone")]
        public string SubMilestone { get; set; }

    }
}
