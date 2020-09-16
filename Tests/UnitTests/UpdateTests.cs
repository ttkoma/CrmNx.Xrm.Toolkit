using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TestFramework;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests
{
    public class UpdateTests
    {
        [Fact]
        public async Task UpdateEntity_Request_Have_HttpMethod_Is_PATCH()
        {
            HttpMethod httpRequestMethod = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestMethod = request.Method;
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["firstname"] = "[TEST]";

            await crmClient.UpdateAsync(entity);

            httpRequestMethod.Should().Be(HttpMethod.Patch);
        }

        [Fact]
        public async Task UpdateEntity_Request_Have_Correct_Uri()
        {
            Uri httpRequestUri = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestUri = request.RequestUri;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["firstname"] = "[TEST]";

            await crmClient.UpdateAsync(entity);

            httpRequestUri.Segments.Last().Should().Be($"contacts({Setup.EntityId})");
        }

        [Fact]
        public async Task UpdateEntity_Request_Have_JSON_ContentType()
        {
            MediaTypeHeaderValue contentType = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                contentType = request.Content.Headers.ContentType;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["firstname"] = "[TEST]";

            await crmClient.UpdateAsync(entity);

            contentType.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task UpdateEntity_When_Set_String_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = "{\"firstname\":\"[TEST]\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestContent = await request.Content.ReadAsStringAsync();
                
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["firstname"] = "[TEST]";

            // Execute
            await crmClient.UpdateAsync(entity);

            // Validate
            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_DateOnly_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = "{\"birthdate\":\"2020-09-15\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["birthdate"] = new DateTime(2020, 09, 15).ToString("yyyy-MM-dd");

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_OptionSet_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = "{\"gendercode\":2,\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["gendercode"] = 2; // Woman

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Money_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = "{\"creditlimit\":300.15,\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["creditlimit"] = 300.15;

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Boolean_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = "{\"creditonhold\":true,\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) => {

                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", Setup.EntityId);
            entity["creditonhold"] = true;

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Lookup_Property_Then_Request_Is_Valid()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task UpdateEntity_When_Set_DateTime_Property_Then_Request_Is_Valid()
        {
            throw new NotImplementedException();
        }

    }
}
