using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class RetrievePrincipalAccessRequestTests
    {
        [Fact]
        public async Task RetrievePrincipalAccessRequest_Query_IsCorrect()
        {
            Uri requestUri = null;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var principal = new EntityReference("systemuser", Guid.NewGuid());
            var accountRef = new EntityReference("account", Guid.NewGuid());
            
            var crmRequest = new RetrievePrincipalAccessRequest(principal, accountRef);

            await crmClient.ExecuteAsync(crmRequest);

            var queryParams = QueryHelpers.ParseQuery(requestUri.Query);
            queryParams.ContainsKey($"@{nameof(crmRequest.Target)}").Should()
                .BeTrue();
            queryParams[$"@{nameof(crmRequest.Target)}"].ToString().Should()
                .Be($"{{\"@odata.id\":\"accounts({accountRef.Id})\"}}");
        }
    }
}