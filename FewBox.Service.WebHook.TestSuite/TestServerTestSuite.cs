using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FewBox.Core.Utility.Formatter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FewBox.Service.WebHook.TestSuite
{
    [TestClass]
    public class TestServerTestSuite
    {
        private IWebHostBuilder WebHostBuilder { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.WebHostBuilder =  new WebHostBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json");
                    })
                    .UseEnvironment("Test")
                    .UseStartup<Startup>();
        }

        [TestMethod]
        public async Task TestAPP()
        {
            await TestServerWapper(async (client)=>{
                string result = await client.GetStringAsync("/api/healthy");
                Assert.AreEqual(@"{""payload"":{""version"":""0.0.1""},""isSuccessful"":true}", result);
            });
        }

        private async Task TestServerWapper(Func<HttpClient, Task> func)
        {
            using (var server = new TestServer(this.WebHostBuilder))
            {
                using (var client = server.CreateClient())
                {
                    await func(client);
                }
            }
        }

        private StringContent ConvertBodyObjectToStringContent<T>(T body)
        {
            string jsonString = JsonUtility.Serialize<T>(body);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }
    }
}