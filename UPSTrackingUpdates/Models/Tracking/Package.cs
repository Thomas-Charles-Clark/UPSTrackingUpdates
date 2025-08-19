using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace UPSTrackingUpdates.Models.Tracking
{
    internal class Package
    {
        [JsonProperty("accessPointInformation")]
        public AccessPointInformation AccessPointInformation { get; set; }

        [JsonProperty("activity")]
        public IEnumerable<Activity> Activity { get; set; }

        [JsonProperty("additionalAttributes")]
        public IEnumerable<string> AdditionalAttributes { get; set; }

        [JsonProperty("additionalServices")]
        public IEnumerable<string> AdditionalServices { get; set; }

        [JsonProperty("alternateTrackingNumber")]
        public AlternateTrackingNumber AlternateTrackingNumber { get; set; }

        [JsonProperty("currentStatus")]
        public Status CurrentStatus { get; set; }

        [JsonProperty("deliveryDate")]
        public IEnumerable<DeliveryDate> DeliveryDate { get; set; }

        [JsonProperty("deliveryInformation")]
        public DeliveryInformation DeliveryInformation { get; set; }

        [JsonProperty("deliveryTime")]
        public DeliveryTime DeliveryTime { get; set; }

        [JsonProperty("dimension")]
        public Dimension Dimension { get; set; }

        [JsonProperty("isSmartPackage")]
        public bool IsSmartPackage { get; set; }

        [JsonProperty("milestones")]
        public IEnumerable<Milestone> Milestones { get; set; }

        [JsonProperty("packageAddress")]
        public IEnumerable<PackageAddress> PackageAddresses { get; set; }

        [JsonProperty("packageCount")]
        public int PackageCount { get; set; }

        [JsonProperty("paymentInformation")]
        public IEnumerable<PaymentInformation> PaymentInformation { get; set; }

        [JsonProperty("referenceNumber")]
        public IEnumerable<ReferenceNumber> ReferenceNumbers { get; set; }

        [JsonProperty("service")]
        public Service Service { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("suppressionIndicators")]
        public string SuppressionIndicators { get; set; }

        [JsonProperty("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("ucixStatus")]
        public string UCIXStatus { get; set; }

        [JsonProperty("weight")]
        public Weight Weight { get; set; }
    }
}
