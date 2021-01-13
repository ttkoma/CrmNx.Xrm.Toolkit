using System;
using System.Collections.Generic;
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
    public class WhoAmITests
    {
        [Fact]
        public async Task WhoAmI_Query_IsCorrect()
        {
            Uri requestUri = null;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var crmRequest = new WhoAmIRequest();

            await crmClient.ExecuteAsync(crmRequest);

            var value = requestUri.Segments.Last();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be("WhoAmI()");
        }
    }
}