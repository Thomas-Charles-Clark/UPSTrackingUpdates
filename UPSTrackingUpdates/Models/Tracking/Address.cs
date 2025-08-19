using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Address
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("addressLine3")]
        public string AddressLine3 { get; set; }

        [JsonProperty(propertyName: "city")]
        public string City { get; set; }

        [JsonProperty(propertyName: "country")]
        public string Country { get; set; }

        [JsonProperty(propertyName: "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(propertyName: "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(propertyName: "stateProvince")]
        public string StateProvince { get; set; }
    }
}
