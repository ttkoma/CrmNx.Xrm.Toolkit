using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing;
using FluentAssertions;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests
{
    public class DisassociateTests
    {
        [Fact]
        public async Task DisassociateAsync_Always_RequestHaveHttpMethodIsDelete()
        {
            HttpMethod httpRequestMethod = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestMethod = request.Method;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var contactRef = new EntityReference("contact", SetupBase.EntityId);

            await crmClient.DisassociateAsync(contactRef, "customerid");

            httpRequestMethod.Should().Be(HttpMethod.Delete);
        }

        [Fact]
        public async Task DisassociateAsync_ReferenceContainsPrimaryId_RequestHaveCorrectUri()
        {
            Uri httpRequestUri = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestUri = request.RequestUri;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entityRef = new EntityReference("contact", SetupBase.EntityId);

            await crmClient.DisassociateAsync(entityRef, "customerid");

            var lastSegment = httpRequestUri.Segments.Length - 1;

            httpRequestUri.Segments[lastSegment - 2].Should().Be($"contacts({SetupBase.EntityId})/");
            httpRequestUri.Segments[lastSegment - 1].Should().Be("customerid/");
            httpRequestUri.Segments[lastSegment].Should().Be("$ref");
        }

        [Fact]
        public async Task DisassociateAsync_ReferenceContainsAlternateKey_RequestHaveCorrectUri()
        {
            Uri httpRequestUri = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestUri = request.RequestUri;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entityRef = new EntityReference("contact", "keyName", "keyValue");

            await crmClient.DisassociateAsync(entityRef, "customerid");

            var lastSegment = httpRequestUri.Segments.Length - 1;

            httpRequestUri.Segments[lastSegment - 2].Should().Be($"contacts(keyName='keyValue')/");
            httpRequestUri.Segments[lastSegment - 1].Should().Be("customerid/");
            httpRequestUri.Segments[lastSegment].Should().Be("$ref");
        }

        /// <summary>
        /// Microsoft.Crm.CrmHttpException: "Versioning is not supported at property level."
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DisassociateAsync_Always_EntityTagHeaderValueIsNull()
        {
            EntityTagHeaderValue httpRequestHeader = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestHeader = request.Headers.IfMatch.FirstOrDefault();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var contactRef = new EntityReference("contact", SetupBase.EntityId);

            await crmClient.DisassociateAsync(contactRef, "customerid");

            httpRequestHeader.Should().Be(default);
        }
    }
}