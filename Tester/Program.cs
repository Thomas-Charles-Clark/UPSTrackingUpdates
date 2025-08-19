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

            var trackingNumber = "1Z5WR5156877515780";

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

            var result = SQLFunctions.FetchTrackingActivity(trackingNumber).OfType<Activity>().ToList();

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

            Console.WriteLine(JsonConvert.SerializeObject(result));
            
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
