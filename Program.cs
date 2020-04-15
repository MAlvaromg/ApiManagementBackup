using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APIManagerBak
{
    class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        static async Task Main(string[] args)
        {
            InitConfiguration();

            AuthenticationContext authContext = new AuthenticationContext(string.Format(Configuration["Instance"], Configuration["TenantId"]));
            AuthenticationResult authResult = await authContext.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(Configuration["ClientId"], Configuration["ClientSecret"]));

            if (authResult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(HttpRequestHeader.Authorization.ToString(), "Bearer " + authResult.AccessToken);
                var uri = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/{3}?api-version=2019-12-01",
                    Configuration["Subscription"],
                    Configuration["ResourceGroup"],
                    Configuration["ApiManagementName"],
                    Configuration["Operation"]);

                var content = JsonConvert.SerializeObject(new
                {
                    storageAccount = Configuration["StorageAccount"],
                    accessKey = Configuration["AccessKey"],
                    containerName = Configuration["ContainerName"],
                    backupName = Configuration["BackupName"]
                });
                var response = await client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
            }
        }

        static void InitConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }
    }
}
