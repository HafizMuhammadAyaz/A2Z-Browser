using A2Z_Browser.Models;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace A2Z_Browser.Services
{
    public class ProxyService
    {
        private readonly DbAccess _dbAccess;

        public ProxyService(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        public async Task<ProxyConfig?> GetProxyConfiguration()
        {
            try
            {
                var query = "SELECT TOP 1 ProxyAddress, Port, Username, Password FROM ProxyConfigs WHERE IsActive = 1 ORDER BY LastVerified DESC";

                var dataTable = await _dbAccess.SelectDataTableAsync(query);

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new ProxyConfig
                    {
                        Address = row["ProxyAddress"].ToString() ?? "",
                        Port = Convert.ToInt32(row["Port"]),
                        Username = row["Username"]?.ToString(),
                        Password = row["Password"]?.ToString()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get proxy configuration error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> VerifyProxy(ProxyConfig proxyConfig)
        {
            try
            {
                var proxy = new WebProxy($"{proxyConfig.Address}:{proxyConfig.Port}");

                if (!string.IsNullOrEmpty(proxyConfig.Username))
                {
                    proxy.Credentials = new NetworkCredential(proxyConfig.Username, proxyConfig.Password);
                }

                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // Test the proxy with a simple request
                    var response = await client.GetAsync("https://httpbin.org/ip");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProxyStatus(ProxyConfig proxyConfig, bool isWorking)
        {
            try
            {
                var query = "UPDATE ProxyConfigs SET LastVerified = GETDATE(), IsActive = @IsActive WHERE ProxyAddress = @Address AND Port = @Port";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@IsActive", isWorking),
                    new SqlParameter("@Address", proxyConfig.Address),
                    new SqlParameter("@Port", proxyConfig.Port)
                };

                var result = await _dbAccess.ExecuteQueryAsync(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update proxy status error: {ex.Message}");
                return false;
            }
        }
    }
}