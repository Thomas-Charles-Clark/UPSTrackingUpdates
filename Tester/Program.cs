using ConsoleFramework;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Data.SqlTypes;
using Newtonsoft.Json.Linq;

using UPSTrackingUpdates;
using UPSTrackingUpdates.Models.Tracking;
using System.Data.SqlClient;

namespace Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            var sw = new Stopwatch();

            Console.OutputEncoding = Encoding.Unicode;

            Framework.SetConsoleColourBusy();

            Framework.WriteHeading("UPS Tracking Info Tester");

            sw.Start();

            //SqlDateTime ActivityDT = SqlDateTime.Null;
            //SqlString StatusCode = SqlString.Null;
            //SqlString StatusDescription = SqlString.Null;
            //SqlString StatusSimplifiedTextDescription = SqlString.Null;
            //SqlString ActivityPackageDetailStatusCode = SqlString.Null;
            //SqlString StatusType = SqlString.Null;
            //SqlString AddressLine1 = SqlString.Null;
            //SqlString AddressLine2 = SqlString.Null;
            //SqlString AddressLine3 = SqlString.Null;
            //SqlString City = SqlString.Null;
            //SqlString Country = SqlString.Null;
            //SqlString CountryCode = SqlString.Null;
            //SqlString PostalCode = SqlString.Null;
            //SqlString StateProvince = SqlString.Null;
            //SqlString SiteLocationIndicatorCode = SqlString.Null;

            var trackingNumbers = new List<string>() { "1Z5WR5156878170758" };

            //using (var conn = new SqlConnection("Data Source=vm-sql;Initial Catalog=Flock;Integrated Security=SSPI"))
            //{
            //     var comm = conn.CreateCommand();

            //comm.CommandText = @"SELECT TOP 10 TrackingNumber FROM Mertex.[ParcelHead]
            //        WHERE HaulierCode = N'UPS' AND CompletedDT < '2025/07/31'
            //        ORDER BY CompletedDT DESC";

            //    conn.Open();

            //    var reader = comm.ExecuteReader();

            //    while(reader.Read())
            //    {
            //        trackingNumbers.Add(reader.GetString(0));
            //    }

            //    conn.Close();
            //}

            foreach (var trackingNumber in trackingNumbers)
            {
                var results = SQLFunctions.FetchTrackingProofOfDelivery(trackingNumber, $"\\\\PC247\\c$\\users\\tclark\\source\\repos\\UPSTrackingUpdates\\Tester\\Documents\\{trackingNumber}").OfType<DeliveryInformation>().ToList();

                SqlString UPSTrackingNumber;
                SqlString ProofOfDeliveryPDFPath;
                SqlBoolean DeliverySignatureIncluded;
                SqlBoolean DeliveryPhotographIncluded;

                foreach (var result in results)
                {
                    SQLFunctions.FetchTrackingProofOfDelivery_FillRow(result, out UPSTrackingNumber, out ProofOfDeliveryPDFPath, out DeliverySignatureIncluded, out DeliveryPhotographIncluded);
                
                    
                }
            }

            //SQLFunctions.FetchTrackingActivity_FillRow(result,
            //    out ActivityDT,
            //    out StatusCode,
            //    out StatusDescription,
            //    out StatusSimplifiedTextDescription,
            //    out ActivityPackageDetailStatusCode,
            //    out StatusType,
            //    out AddressLine1,
            //    out AddressLine2,
            //    out AddressLine3,
            //    out City,
            //    out Country,
            //    out CountryCode,
            //    out PostalCode,
            //    out StateProvince,
            //    out SiteLocationIndicatorCode
            //);

            //Console.WriteLine(JsonConvert.SerializeObject(result));
            
            Console.WriteLine();

            sw.Stop();

            Framework.SetConsoleColourSuccess();

            Console.WriteLine($"Complete in {sw.Elapsed}");

            Console.WriteLine();

            Console.WriteLine("Press any key to continue...");

            Console.ReadKey();
        }
    }
}
