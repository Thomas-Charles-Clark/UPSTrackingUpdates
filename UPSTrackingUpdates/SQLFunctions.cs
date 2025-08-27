using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.SqlServer.Server;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UPSTrackingUpdates.Models;
using UPSTrackingUpdates.Models.Tracking;
using UPSTrackingUpdates.Utilities;

namespace UPSTrackingUpdates
{
    public static class SQLFunctions
    {
        private static readonly string upsOAuthEndpoint = "https://onlinetools.ups.com/security/v1/oauth/token";
        private static readonly string upsTrackingEndpoint = "https://onlinetools.ups.com/api/track/v1/details";
        private static readonly Regex upsLogoRegex = new Regex("(src=\"\\.)");

        [SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "FetchTrackingInfo_FillRow")]
        public static IEnumerable FetchTrackingInfo(string UPSTrackingNumber)
        {
            Framework.SetTLS();

            var results = new ArrayList();

            var credentials = Framework.GetCredentials();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-merchant-id", credentials.AccountNumber);

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.ClientID}:{credentials.ClientSecret}"));

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64String);

                var oauthRequest = client.PostAsync(upsOAuthEndpoint, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })).Result;

                var oauthResponse = oauthRequest.Content.ReadAsStringAsync().Result;

                var oauthTokenResponse = JsonConvert.DeserializeObject<OAuthResponse>(oauthResponse);

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {oauthTokenResponse.AccessToken}");

                client.DefaultRequestHeaders.Add("Accept", "application/json");

                client.DefaultRequestHeaders.Add("transId", Guid.NewGuid().ToString());

                client.DefaultRequestHeaders.Add("transactionSrc", $"UPSTrackingUpdates-JL");

                var requestUrl = $"{upsTrackingEndpoint}/{UPSTrackingNumber}";

                var response = client.GetAsync(requestUrl).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                var responseJsonObject = JObject.Parse(responseString);

#if DEBUG
                Console.WriteLine(responseJsonObject);
#endif

                if (response.IsSuccessStatusCode)
                {
                    results.Add(responseJsonObject.ToObject<TrackResponseWrapper>());
                }
                else
                {
                    throw new Exception(responseString);
                }
            }

            return results;
        }

        public static void FetchTrackingInfo_FillRow(
            object o,
            out SqlString MostRecentStatus,
            out SqlDateTime MostRecentActivityDT,
            out SqlString ResponseJSON)
        {
            var p = (TrackResponseWrapper)o;

            if (p != null)
            {
                var currentStatus = p.TrackResponse?.Shipments?.FirstOrDefault()?.Packages?.FirstOrDefault().CurrentStatus;

                MostRecentStatus = currentStatus?.SimplifiedTextDescription ?? currentStatus.Description ?? SqlString.Null;
                MostRecentActivityDT = p.TrackResponse?.Shipments?.FirstOrDefault()?.Packages?.FirstOrDefault()?.Activity?.FirstOrDefault().ActivityDT ?? SqlDateTime.Null;
                ResponseJSON = JsonConvert.SerializeObject(o);
            }
            else
            {
                MostRecentStatus = SqlString.Null;
                MostRecentActivityDT = SqlDateTime.Null;
                ResponseJSON = SqlString.Null;
            }
        }


        [SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "FetchTrackingActivity_FillRow")]
        public static IEnumerable FetchTrackingActivity(string UPSTrackingNumber, string ProofOfDeliveryDestinationFolder)
        {
            Framework.SetTLS();

            var results = new ArrayList();

            var credentials = Framework.GetCredentials();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-merchant-id", credentials.AccountNumber);

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.ClientID}:{credentials.ClientSecret}"));

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64String);

                var oauthRequest = client.PostAsync(upsOAuthEndpoint, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })).Result;

                var oauthResponse = oauthRequest.Content.ReadAsStringAsync().Result;

                var oauthTokenResponse = JsonConvert.DeserializeObject<OAuthResponse>(oauthResponse);

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {oauthTokenResponse.AccessToken}");

                client.DefaultRequestHeaders.Add("Accept", "application/json");

                client.DefaultRequestHeaders.Add("transId", Guid.NewGuid().ToString());

                client.DefaultRequestHeaders.Add("transactionSrc", $"UPSTrackingUpdates-JL");

                var requestUrl = $"{upsTrackingEndpoint}/{UPSTrackingNumber}?returnSignature=true&returnPOD=true";

                var response = client.GetAsync(requestUrl).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                var responseJsonObject = JObject.Parse(responseString);

//#if DEBUG
//                Console.WriteLine(responseJsonObject);
//#endif

                if (response.IsSuccessStatusCode)
                {
#if DEBUG
                    Console.WriteLine($"Processing {UPSTrackingNumber}");
#endif

                    var trackingResponseWrapper = responseJsonObject.ToObject<TrackResponseWrapper>();

                    foreach(var p in trackingResponseWrapper.TrackResponse.Shipments?.SelectMany(q => q.Packages?.SelectMany(j => j.Activity)))
                    {
                        p.UPSTrackingNumber = UPSTrackingNumber;

                        results.Add(p);
                    }

                    if (!String.IsNullOrEmpty(ProofOfDeliveryDestinationFolder))
                    {
                        if (!Directory.Exists(ProofOfDeliveryDestinationFolder))
                        {
                            Directory.CreateDirectory(ProofOfDeliveryDestinationFolder);
                        }

                        trackingResponseWrapper.TrackResponse.Shipments?.SelectMany(q => q.Packages).ToList().ForEach(p => 
                        { 
                            if (!String.IsNullOrEmpty(p.DeliveryInformation?.Signature?.Base64EncodedImage))
                            {
                                var deliverySignatureBytes = Convert.FromBase64String(p.DeliveryInformation?.Signature?.Base64EncodedImage);

                                var deliverySignatureFileName = $"{ProofOfDeliveryDestinationFolder}\\DeliverySignature.{ImageDetector.GetImageType(deliverySignatureBytes).ToString().ToLowerInvariant()}";

                                Console.WriteLine($"Wrote {deliverySignatureFileName}");

                                File.WriteAllBytes(deliverySignatureFileName, deliverySignatureBytes);
                            }

                            if (!String.IsNullOrEmpty(p.DeliveryInformation?.ProofOfDelivery?.Base64EncodedImage))
                            {
                                var proofOfDeliveryBytes = Convert.FromBase64String(p.DeliveryInformation?.ProofOfDelivery?.Base64EncodedImage);

                                var proofOfDeliveryHtml = Encoding.UTF8.GetString(proofOfDeliveryBytes).Trim();

                                proofOfDeliveryHtml = upsLogoRegex.Replace(proofOfDeliveryHtml, "src=\"..");

                                var proofOfDeliveryFileName = $"{ProofOfDeliveryDestinationFolder}\\ProofOfDelivery.html";

                                File.WriteAllText(proofOfDeliveryFileName, proofOfDeliveryHtml);

                                Console.WriteLine($"Wrote {proofOfDeliveryFileName}");
                            }

                            if (!String.IsNullOrEmpty(p.DeliveryInformation?.DeliveryPhoto?.Base64EncodedContent))
                            {
                                var deliveryPhotoBytes = Convert.FromBase64String(p.DeliveryInformation?.DeliveryPhoto?.Base64EncodedContent);

                                var deliveryPhotoBytesFileName = $"{ProofOfDeliveryDestinationFolder}\\DeliveryPhoto.{ImageDetector.GetImageType(deliveryPhotoBytes).ToString().ToLowerInvariant()}";

                                File.WriteAllBytes(deliveryPhotoBytesFileName, deliveryPhotoBytes);

                                Console.WriteLine($"Wrote {deliveryPhotoBytesFileName}");
                            }
                        });
                    }
                }
                else
                {
                    throw new Exception(responseString);
                }
            }

            return results;
        }

        public static void FetchTrackingActivity_FillRow(
            object o,
            out SqlString UPSTrackingNumber,
            out SqlDateTime ActivityDT,
            out SqlString StatusCode,
            out SqlString StatusDescription,
            out SqlString StatusSimplifiedTextDescription,
            out SqlString ActivityPackageDetailStatusCode,
            out SqlString StatusType,
            out SqlString AddressLine1,
            out SqlString AddressLine2,
            out SqlString AddressLine3,
            out SqlString City,
            out SqlString Country,
            out SqlString CountryCode,
            out SqlString PostalCode,
            out SqlString StateProvince,
            out SqlString SiteLocationIndicatorCode
            )
        {
            var p = (Activity)o;

            UPSTrackingNumber = p.UPSTrackingNumber;
            ActivityDT = p.ActivityDT ?? SqlDateTime.Null;
            StatusCode = p.Status?.Code ?? SqlString.Null;
            StatusDescription = p.Status?.Description ?? SqlString.Null;
            StatusSimplifiedTextDescription = p.Status?.SimplifiedTextDescription ?? SqlString.Null;
            ActivityPackageDetailStatusCode = p.Status?.StatusCode ?? SqlString.Null;
            StatusType = p.Status?.Type ?? SqlString.Null;
            AddressLine1 = p.Location?.Address?.AddressLine1 ?? SqlString.Null;
            AddressLine2 = p.Location?.Address?.AddressLine2 ?? SqlString.Null;
            AddressLine3 = p.Location?.Address?.AddressLine3 ?? SqlString.Null;
            City = p.Location?.Address?.City ?? SqlString.Null;
            Country = p.Location?.Address?.Country ?? SqlString.Null;
            CountryCode = p.Location?.Address?.CountryCode ?? SqlString.Null;
            PostalCode = p.Location?.Address?.PostalCode ?? SqlString.Null;
            StateProvince = p.Location?.Address?.StateProvince ?? SqlString.Null;
            SiteLocationIndicatorCode = p.Location?.SiteLocationIndicatorCode ?? SqlString.Null;
        }
    }
}
