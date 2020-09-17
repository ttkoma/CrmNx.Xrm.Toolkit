using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using TestFramework;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class GetAllTimeZonesWithDisplayNameTests
    {
        [Fact]
        public async Task GetAllTimeZonesWithDisplayName_Deserialize_Correct()
        {
            const string jsonContent = @"
            {
                ""value"": [ 
                    {
                        ""timezonecode"": 0,
                        ""timezonedefinitionid"": ""30343341-5cdc-4def-a8ac-7d76187548d2"",
                        ""versionnumber"": 743832113,
                        ""userinterfacename"":""(GMT-12:00) International Date Line West"",
                        ""bias"": 720
                    },
                    {
                        ""timezonecode"": 1,
                        ""timezonedefinitionid"": ""a6d443d7-e9a0-406e-86a2-853d65d17098"",
                        ""versionnumber"": 743832117,
                        ""userinterfacename"": ""(GMT+13:00) Samoa"",
                        ""bias"": -780
                    },
                    {
                        ""timezonecode"": 2,
                        ""timezonedefinitionid"": ""fa16509c-06a9-4ef1-8cb1-883f812f74e1"",
                        ""versionnumber"": 743832127,
                        ""userinterfacename"": ""(GMT-10:00) Hawaii"",
                        ""bias"": 600
                    }
                ]
            }";

            var apiResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));
            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var apiRequest = new GetAllTimeZonesWithDisplayNameRequest(localeId: 1033);
            var result = await crmClient.ExecuteAsync<GetAllTimeZonesWithDisplayNameResponse>(apiRequest);

            result.Items.Should().HaveCount(3);

            result.Items.First(x => x.TimezoneCode == 0).Should().NotBeNull();
            result.Items.First(x => x.TimezoneCode == 1).Should().NotBeNull();
            result.Items.First(x => x.TimezoneCode == 2).Should().NotBeNull();

            result.Items.First(x => x.TimezoneCode == 2).Bias.Should().Be(600);
            result.Items.First(x => x.TimezoneCode == 2).UserInterfaceName.Should().Be("(GMT-10:00) Hawaii");
            result.Items.First(x => x.TimezoneCode == 2).TimezoneDefinitioId.Should()
                .Be(Guid.Parse("fa16509c-06a9-4ef1-8cb1-883f812f74e1"));
        }

        [Fact()]
        public void GetAllTimeZonesWithDisplayName_When_LocaleId_Present_Query_IsCorrect()
        {
            var crmRequest = new GetAllTimeZonesWithDisplayNameRequest(1033);

            var query = crmRequest.QueryString();

            query.Should()
                .Be(
                    "timezonedefinitions/Microsoft.Dynamics.CRM.GetAllTimeZonesWithDisplayName(LocaleId=@LocaleId)?@LocaleId=1033");
        }
    }
}