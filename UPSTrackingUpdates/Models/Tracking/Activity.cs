using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Activity
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("gmtDate")]
        public string GMTDate { get; set; }

        [JsonProperty("gmtOffset")]
        public string GMTOffset { get; set; }

        [JsonProperty("gmtTime")]
        public string GMTTime { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        public string UPSTrackingNumber { get; set; }

        public DateTime? ActivityDT
        {
            get
            {
                if (!String.IsNullOrEmpty(GMTDate) &&
                    !String.IsNullOrEmpty(GMTTime) &&
                    !String.IsNullOrEmpty(GMTOffset))
                {
                    return DateTime.ParseExact($"{GMTDate} {GMTTime} {GMTOffset}", "yyyyMMdd HH:mm:ss zzz", new System.Globalization.CultureInfo("en-GB").DateTimeFormat);
                }
                else if (!String.IsNullOrEmpty(Date) &&
                    !String.IsNullOrEmpty(Time))
                {
                    return DateTime.ParseExact($"{Date} {Time}", "yyyyMMdd HH:mm:ss", new System.Globalization.CultureInfo("en-GB").DateTimeFormat);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
