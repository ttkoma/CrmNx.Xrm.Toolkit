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
    public class QueryScheduleTests
    {
        [Fact]
        public async Task QuerySchedules_ToQueryString_Start_IsCorrect()
        {
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var crmRequest = new QueryScheduleRequest()
            {
                Start = new DateTime(2019, 02, 25, 0, 0, 0, DateTimeKind.Utc),
            };

            await crmClient.ExecuteAsync(crmRequest);
            
            var value = QueryHelpers.ParseQuery(requestUri.Query)
                .GetValueOrDefault("@Start").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be(crmRequest.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        [Fact]
        public async Task QuerySchedules_ToQueryString_End_IsCorrect()
        {
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var crmRequest = new QueryScheduleRequest()
            {
                End = new DateTime(2019, 02, 25, 0, 0, 0, DateTimeKind.Local),
            };
            
            await crmClient.ExecuteAsync(crmRequest);
            var value = QueryHelpers.ParseQuery(requestUri.Query).GetValueOrDefault("@End").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be(crmRequest.End.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        [Fact]
        public async Task QuerySchedules_ToQueryString_ResourceId_IsCorrect()
        {
            
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            // Create
            var crmRequest = new QueryScheduleRequest()
            {
                ResourceId = SetupBase.EntityId
            };
            
            await crmClient.ExecuteAsync(crmRequest);
            
            // Test
            var value = QueryHelpers.ParseQuery(requestUri.Query).GetValueOrDefault("@ResourceId").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"{SetupBase.EntityId}");
        }

        [Fact]
        public async Task QuerySchedules_ToQueryString_When_Empty_TimeCodes_IsCorrect()
        {
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var crmRequest = new QueryScheduleRequest();

            await crmClient.ExecuteAsync(crmRequest);
            
            // Test
            var value = QueryHelpers.ParseQuery(requestUri.Query).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be("[]");
        }

        [Fact]
        public async Task QuerySchedules_ToQueryString_When_One_TimeCodes_IsCorrect()
        {
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var crmRequest = new QueryScheduleRequest()
            {
                TimeCodes = new[] {TimeCode.Filter}
            };

            await crmClient.ExecuteAsync(crmRequest);
            
            // Test
            var value = QueryHelpers.ParseQuery(requestUri.Query).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"{(int) TimeCode.Filter}\"]");
        }

        [Fact]
        public async Task QuerySchedules_ToQueryString_When_Multiple_TimeCodes_IsCorrect()
        {
            Uri requestUri = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var crmRequest = new QueryScheduleRequest()
            {
                TimeCodes = new[] {TimeCode.Filter, TimeCode.Available, TimeCode.Busy}
            };

            await crmClient.ExecuteAsync(crmRequest);
            
            var value = QueryHelpers.ParseQuery(requestUri.Query).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"3\",\"0\",\"1\"]");
        }
    }
}