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
    public class UpdateTests
    {
        [Fact]
        public async Task UpdateEntity_Request_Have_HttpMethod_Is_PATCH()
        {
            HttpMethod httpRequestMethod = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                httpRequestMethod = request.Method;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["firstname"] = "[TEST]"
            };

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

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["firstname"] = "[TEST]";

            await crmClient.UpdateAsync(entity);

            httpRequestUri.Segments.Last().Should().Be($"contacts({SetupBase.EntityId})");
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

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["firstname"] = "[TEST]";

            await crmClient.UpdateAsync(entity);

            contentType.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task UpdateEntity_When_Set_String_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent =
"{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"firstname\":\"[TEST]\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";

            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["firstname"] = "[TEST]";

            // Execute
            await crmClient.UpdateAsync(entity);

            // Validate
            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_DateOnly_Property_Then_Request_Is_Valid()
        {
            const string expectedRequestContent =
@"{""@odata.type"":""Microsoft.Dynamics.CRM.contact"",""birthdate"":""2020-09-15"",""contactid"":""00000000-0000-0000-0000-000000000001""}";
            
            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["birthdate"] = new DateTime(2020, 09, 15)
            };

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_OptionSet_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent = 
"{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"gendercode\":2,\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            
            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["gendercode"] = 2; // Woman

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Money_Property_Then_Request_Is_Valid()
        {
            string expectedRequestContent =
"{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"creditlimit\":300.15,\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";
            string httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["creditlimit"] = 300.15;

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Boolean_Property_Then_Request_Is_Valid()
        {
            const string expectedRequestContent =
@"{""@odata.type"":""Microsoft.Dynamics.CRM.contact"",""creditonhold"":true,""contactid"":""00000000-0000-0000-0000-000000000001""}";
            
            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId);
            entity["creditonhold"] = true;

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_Lookup_Property_Then_Request_Is_Valid()
        {
            const string expectedRequestContent =
"{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"ownerid@odata.bind\":\"systemusers(00000000-0000-0000-0000-000000000001)\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}";

            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["ownerid"] = new EntityReference("systemuser", SetupBase.EntityId)
            };

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_DateTime_Property_As_UtcKind_Then_Request_Is_Valid()
        {
            var utcDateTime = new DateTime(2020, 09, 15, 0, 0, 0, DateTimeKind.Utc);

            var expectedRequestContent =
$"{{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"modifiedon\":\"{utcDateTime:yyyy-MM-ddTHH:mm:ssZ}\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}}";

            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["modifiedon"] = utcDateTime
            };

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_DateTime_Property_As_UnspecifiedKind_Then_Request_Is_Valid()
        {
            var unspecifiedDateTime = new DateTime(2020, 09, 15, 0, 0, 0, DateTimeKind.Unspecified);

            var expectedRequestContent =
$"{{\"@odata.type\":\"Microsoft.Dynamics.CRM.contact\",\"modifiedon\":\"{unspecifiedDateTime:yyyy-MM-ddTHH:mm:ssZ}\",\"contactid\":\"00000000-0000-0000-0000-000000000001\"}}";
            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["modifiedon"] = unspecifiedDateTime
            };

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_Set_DateTime_Property_As_LocalKind_Then_Request_Is_Valid()
        {
            var localDateTime = new DateTime(2020, 09, 15, 0, 0, 0, DateTimeKind.Local);

            var expectedRequestContent =
@$"{{""@odata.type"":""Microsoft.Dynamics.CRM.contact"",""modifiedon"":""{localDateTime.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"",""contactid"":""00000000-0000-0000-0000-000000000001""}}";
            var httpRequestContent = string.Empty;

            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("contact", SetupBase.EntityId)
            {
                ["modifiedon"] = localDateTime
            };

            await crmClient.UpdateAsync(entity);

            httpRequestContent.Should().Be(expectedRequestContent);
        }

        [Fact]
        public async Task UpdateEntity_When_RowVersion_IsStringNumber_Then_Request_Is_Valid()
        {
            HttpRequestHeaders httpRequestHeaders = default;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestHeaders = request.Headers;

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var entity = new Entity("account")
            {
                RowVersion = "723316"
            };

            await crmClient.UpdateAsync(entity);

            httpRequestHeaders.IfMatch.Count.Should().BeGreaterOrEqualTo(1);
            httpRequestHeaders.IfMatch.ElementAt(0).IsWeak.Should().BeTrue();
            httpRequestHeaders.IfMatch.ElementAt(0).Tag.Should().Be("\"723316\"");

        }
        
        [Fact]
        public async Task UpdateEntity_When_RowVersion_IsQuotedString_Then_Request_Is_Valid()
        {
            HttpRequestHeaders httpRequestHeaders = default;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestHeaders = request.Headers;

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var entity = new Entity("account")
            {
                RowVersion = "\"723316\""
            };

            await crmClient.UpdateAsync(entity);

            httpRequestHeaders.IfMatch.Count.Should().BeGreaterOrEqualTo(1);
            httpRequestHeaders.IfMatch.ElementAt(0).IsWeak.Should().BeTrue();
            httpRequestHeaders.IfMatch.ElementAt(0).Tag.Should().Be("\"723316\"");

        }
        
        [Fact]
        public async Task UpdateEntity_When_RowVersion_IsODataETag_Then_Request_Is_Valid()
        {
            HttpRequestHeaders httpRequestHeaders = default;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestHeaders = request.Headers;

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            var entity = new Entity("account")
            {
                RowVersion = "W/\"723316\""
            };

            await crmClient.UpdateAsync(entity);

            httpRequestHeaders.IfMatch.Count.Should().BeGreaterOrEqualTo(1);
            httpRequestHeaders.IfMatch.ElementAt(0).IsWeak.Should().BeTrue();
            httpRequestHeaders.IfMatch.ElementAt(0).Tag.Should().Be("\"723316\"");

        }
        
        [Fact]
        public async Task UpdateEntity_When_RowVersion_NotPresent_Then_Upsert_Is_Disabled()
        {
            HttpRequestHeaders httpRequestHeaders = default;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestHeaders = request.Headers;

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("account");

            await crmClient.UpdateAsync(entity);

            httpRequestHeaders.IfMatch.Count.Should().BeGreaterOrEqualTo(1);
            httpRequestHeaders.IfMatch.ElementAt(0).IsWeak.Should().BeFalse();
            httpRequestHeaders.IfMatch.ElementAt(0).Tag.Should().Be("*");
        }
        
        [Fact]
        public async Task UpdateEntity_When_RowVersion_NotPresent_And_AllowUpsert_IsTrue_Then_IfMathHeader_Is_Empty()
        {
            HttpRequestHeaders httpRequestHeaders = default;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                httpRequestHeaders = request.Headers;

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("account");

            await crmClient.UpdateAsync(entity, allowUpsert: true);

            httpRequestHeaders.IfMatch.Count.Should().Be(0);
        }
    }
}