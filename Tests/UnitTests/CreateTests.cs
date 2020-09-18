using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Xrm.Toolkit.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests
{
    public class CreateTests
    {
        [Fact]
        public async Task CreateAsync_Then_EntityId_Parsed_From_Response_Headers()
        {
            var apiResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            apiResponse.Headers.Add(
                "OData-EntityId", $"https://{SetupBase.D365CeHttpClientBaseAddress}/accounts({SetupBase.EntityId})");

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));
            var client = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("account");
            var result = await client.CreateAsync(entity);

            result.Should().NotBeEmpty();
            result.Should().Be(SetupBase.EntityId);
        }

        [Fact]
        public void CreateAsync_When_Duplicated_ById_Then_Throw_Exception()
        {
            var crmResponse = @"
            {
                ""error"": {
                    ""code"": """",
                    ""message"": ""A record with matching key values already exists."",
                    ""innererror"": {
                        ""message"": ""Duplicate Record Found for Entity: 1 with ID: { + " + SetupBase.EntityId +
                              @"}"",
                        ""type"" : """",
                        ""stacktrace"" : """"
                    }
                }
            }";

            var apiResponse = new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
            {
                Content = new StringContent(crmResponse, Encoding.UTF8, "application/json")
            };

            apiResponse.Content.Headers.ContentType.Parameters.Add(
                new NameValueHeaderValue("odata.metadata", "minimal"));

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));

            var client = FakeCrmWebApiClient.Create(httpClient);

            var entity = new Entity("account")
            {
                Id = SetupBase.EntityId
            };

            Func<Task> createAction = () => client.CreateAsync(entity);

            createAction.Should()
                .Throw<WebApiException>()
                .WithMessage("A record with matching key values already exists.")
                .And.StatusCode.Should().Be((int) HttpStatusCode.PreconditionFailed);
        }
    }
}