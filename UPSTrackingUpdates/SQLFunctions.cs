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

using HtmlAgilityPack;
using iTextSharp.tool.xml.html;

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
        public static IEnumerable FetchTrackingActivity(string UPSTrackingNumber)
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

                client.DefaultRequestHeaders.Add("transactionSrc", $"UPSTrackingUpdates");

                var requestUrl = $"{upsTrackingEndpoint}/{UPSTrackingNumber}";

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

                    foreach (var p in trackingResponseWrapper.TrackResponse.Shipments?.SelectMany(q => q.Packages?.SelectMany(j => j.Activity)))
                    {
                        p.UPSTrackingNumber = UPSTrackingNumber;

                        results.Add(p);
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

        /// <param name="FileName">
        ///     File Name
        /// </param>
        /// <param name="UPSTrackingNumber">
        ///     Tracking Number
        /// </param>
        /// <param name="ProofOfDeliveryDestinationFolder">
        ///     Document Store
        /// </param>
        /// <returns>
        ///     Record concerning destination and proof of delivery signature / photo inclusion
        /// </returns>
        /// <exception cref="Exception"></exception>
        [SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "FetchTrackingProofOfDelivery_FillRow")]
        public static IEnumerable FetchTrackingProofOfDelivery(string FileName, string UPSTrackingNumber, string ProofOfDeliveryDestinationFolder)
        {
            Framework.SetTLS();

            var results = new ArrayList();

            var credentials = Framework.GetCredentials();

            bool deliverySignatureIncluded = false;

            bool deliveryPhotoIncluded = false;

            bool deliveryProofIncluded = false;

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

                client.DefaultRequestHeaders.Add("transactionSrc", $"UPSTrackingUpdates");

                var requestUrl = $"{upsTrackingEndpoint}/{UPSTrackingNumber}?returnSignature=true&returnPOD=true";

                var response = client.GetAsync(requestUrl).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                var responseJsonObject = JObject.Parse(responseString);

                if (response.IsSuccessStatusCode)
                {
                    var trackingResponseWrapper = responseJsonObject.ToObject<TrackResponseWrapper>();

                    var proofOfDeliveryPDFPath = (string)null;

                    if (!String.IsNullOrEmpty(ProofOfDeliveryDestinationFolder))
                    {
                        if (!Directory.Exists(ProofOfDeliveryDestinationFolder))
                        {
                            Directory.CreateDirectory(ProofOfDeliveryDestinationFolder);
                        }

                        if (trackingResponseWrapper.TrackResponse.Shipments?.Where(q => q.Packages != null)?.Any() == true)
						{
							trackingResponseWrapper.TrackResponse.Shipments?.SelectMany(q => q.Packages).ToList().ForEach(p =>
							{
								var deliverySignatureBytes = new byte[] { };
								var proofOfDeliveryBytes = new byte[] { };

								if (!String.IsNullOrEmpty(p.DeliveryInformation?.Signature?.Base64EncodedImage))
								{
									deliverySignatureBytes = Convert.FromBase64String(p.DeliveryInformation?.Signature?.Base64EncodedImage);

									deliverySignatureIncluded = true;
								}

								if (!String.IsNullOrEmpty(p.DeliveryInformation?.ProofOfDelivery?.Base64EncodedHTML))
								{
									proofOfDeliveryBytes = Convert.FromBase64String(p.DeliveryInformation?.ProofOfDelivery?.Base64EncodedHTML);

									var proofOfDeliveryHtml = Encoding.UTF8.GetString(proofOfDeliveryBytes).Trim();

									var agilityPackDocument = new HtmlDocument();

									int f = 0;

									// Configure parsing options
									agilityPackDocument.OptionFixNestedTags = true;
									agilityPackDocument.OptionAutoCloseOnEnd = true;
									agilityPackDocument.OptionWriteEmptyNodes = true;

									agilityPackDocument.LoadHtml(proofOfDeliveryHtml);

									proofOfDeliveryHtml = agilityPackDocument.DocumentNode.OuterHtml;

									proofOfDeliveryHtml = upsLogoRegex.Replace(proofOfDeliveryHtml, "src=\"..");

									var proofOfDeliveryFileNamePDF = $"{ProofOfDeliveryDestinationFolder}\\{FileName}.pdf";

									deliveryProofIncluded = true;

									PDFWriter.WritePDF(proofOfDeliveryHtml, proofOfDeliveryFileNamePDF, deliverySignatureBytes);

									proofOfDeliveryPDFPath = proofOfDeliveryFileNamePDF;
								}

								p.DeliveryInformation.UPSTrackingNumber = UPSTrackingNumber;
								p.DeliveryInformation.ProofOfDeliveryPDFPath = proofOfDeliveryPDFPath;
								p.DeliveryInformation.DeliverySignatureIncluded = deliverySignatureIncluded;
								p.DeliveryInformation.DeliveryPhotographIncluded = deliveryPhotoIncluded;

								results.Add(p.DeliveryInformation);
							});
						}
                        else
                        {
                            var warnings = trackingResponseWrapper.TrackResponse.Shipments?.SelectMany(p => p.Warnings);

							if (warnings.Any())
                            {
                                results.Add(new DeliveryInformation()
                                {
                                    UPSTrackingNumber = UPSTrackingNumber,
                                    ProofOfDeliveryPDFPath = null,
                                    DeliverySignatureIncluded = false,
                                    DeliveryPhotographIncluded = false
                                });
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception(responseString);
                }
            }

            return results;
        }
        public static void FetchTrackingProofOfDelivery_FillRow(
            object o,
            out SqlString UPSTrackingNumber,
            out SqlString ProofOfDeliveryPDFPath,
            out SqlBoolean DeliverySignatureIncluded,
            out SqlBoolean DeliveryPhotographIncluded
            )
        {
            var p = (DeliveryInformation)o;

            UPSTrackingNumber = p.UPSTrackingNumber;
            ProofOfDeliveryPDFPath = String.IsNullOrEmpty(p.ProofOfDeliveryPDFPath) ? SqlString.Null : p.ProofOfDeliveryPDFPath;
            DeliverySignatureIncluded = p.DeliverySignatureIncluded;
            DeliveryPhotographIncluded = p.DeliveryPhotographIncluded;
        }
    }
}
