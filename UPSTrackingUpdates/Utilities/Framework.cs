using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using UPSTrackingUpdates.Models;

namespace UPSTrackingUpdates.Utilities
{
    internal static partial class Framework
    {
        public static void SetTLS()
        {
            try
            { //try TLS 1.3
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288
                                                     | (SecurityProtocolType)3072
                                                     | (SecurityProtocolType)768
                                                     | SecurityProtocolType.Tls;
            }
            catch (NotSupportedException)
            {
                try
                { //try TLS 1.2
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072
                                                         | (SecurityProtocolType)768
                                                         | SecurityProtocolType.Tls;
                }
                catch (NotSupportedException)
                {
                    try
                    { //try TLS 1.1
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768
                                                             | SecurityProtocolType.Tls;
                    }
                    catch (NotSupportedException)
                    { //TLS 1.0
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    }
                }
            }
        }

        public static Credentials GetCredentials()
        {
#if DEBUG
            using (var conn = new SqlConnection("Data Source=vm-sql;Initial Catalog=Flock;Integrated Security=True"))
#else
            using (var conn = new SqlConnection("context connection=true"))
#endif
            {
                var result = conn.QuerySingle<Credentials>("SELECT AccountNumber, ClientID, ClientSecret FROM ups.App WHERE AppID = @AppID", new
                {
                    AppID = 2
                }, null, 30, CommandType.Text);

                return result;
            }
        }
    }
}
