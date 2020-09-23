using System;
using System.Net.Http;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrmNx.Crm.Toolkit.Testing
{
    public class FakeCrmWebApiClient : CrmWebApiClient
    {
        public string BaseAddress
        {
            get => HttpClient.BaseAddress.ToString();
            set => HttpClient.BaseAddress = new Uri(value);
        }

        public FakeCrmWebApiClient(HttpClient httpClient, IWebApiMetadataService metadata)
            : base(httpClient, metadata, NullLogger<FakeCrmWebApiClient>.Instance)
        {
        }

        /// <summary>
        /// Create instance of faked CrmWebApiClient with default Micorosft 365 CE metadata definitions
        /// </summary>
        /// <param name="httpClient">Mocked httpHandler</param>
        /// <returns></returns>
        public static FakeCrmWebApiClient Create(HttpClient httpClient)
        {
            var metadata = MockedWebApiMetadata.CreateD365Ce();
            return new FakeCrmWebApiClient(httpClient, metadata)
            {
                BaseAddress = SetupBase.D365CeHttpClientBaseAddress.ToString()
            };
        }
    }
}