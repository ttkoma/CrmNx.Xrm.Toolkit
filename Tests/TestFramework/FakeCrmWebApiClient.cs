using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;

namespace TestFramework
{
    public class FakeCrmWebApiClient : CrmWebApiClient
    {
        public FakeCrmWebApiClient(HttpClient httpClient, WebApiMetadata metadata)
            : base(httpClient, metadata, NullLogger<FakeCrmWebApiClient>.Instance) { }

        /// <summary>
        /// Create instance of faked CrmWebApiClient with default Micorosft 365 CE metadata definitions
        /// </summary>
        /// <param name="httpClient">Mocked httpHandler</param>
        /// <returns></returns>
        public static FakeCrmWebApiClient Create(HttpClient httpClient)
        {
            var metadata = MockedWebApiMetadata.CreateD365CE();
            httpClient.BaseAddress = Setup.D365CeHttpClientBaseAddress;

            return new FakeCrmWebApiClient(httpClient, metadata);
        }
    }
}
