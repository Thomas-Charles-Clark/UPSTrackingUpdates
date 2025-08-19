using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class AccessPointInformation
    {
        [JsonProperty("pickupByDate")]
        public string PickupByDate { get; set; }

        public DateTime? PickupByDT
        {
            get
            {
                if (!String.IsNullOrEmpty(PickupByDate))
                {
                    if (DateTime.TryParse(PickupByDate, out DateTime r))
                    {
                        return r;
                    }
                }

                return null;
            }
        }
    }
}
