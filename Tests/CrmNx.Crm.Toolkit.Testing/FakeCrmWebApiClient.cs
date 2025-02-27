using System;
using System.Net.Http;
using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CrmNx.Crm.Toolkit.Testing
{
    public class FakeCrmWebApiClient : CrmWebApiClient
    {
        public FakeCrmWebApiClient(HttpClient httpClient, IWebApiMetadataService metadata, IOptions<CrmClientSettings> options)
            : base(httpClient, metadata, options, NullLogger<FakeCrmWebApiClient>.Instance)
        {
        }

        /// <summary>
        /// Create instance of faked CrmWebApiClient with default Micorosft 365 CE metadata definitions
        /// </summary>
        /// <param name="httpClient">Mocked httpHandler</param>
        /// <returns></returns>
        public static FakeCrmWebApiClient Create(HttpClient httpClient)
        {
            string connectionString = "Url=http://host.local/demo;Username=;Password=;Domain=;Authtype=AD";
            
            var metadata = MockedWebApiMetadata.CreateD365Ce();
            var options = Options.Create(new CrmClientSettings()
            {
                ConnectionString = connectionString
            });
                
            return new FakeCrmWebApiClient(httpClient, metadata, options)
            {
                // BaseAddress = SetupBase.D365CeHttpClientBaseAddress.ToString()
            };
        }
    }
}