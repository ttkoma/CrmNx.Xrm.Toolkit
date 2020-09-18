using System.Net.Http;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrmNx.Crm.Toolkit.Testing
{
    public class FakeCrmWebApiClient : CrmWebApiClient
    {
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
            httpClient.BaseAddress = SetupBase.D365CeHttpClientBaseAddress;

            return new FakeCrmWebApiClient(httpClient, metadata);
        }
    }
}