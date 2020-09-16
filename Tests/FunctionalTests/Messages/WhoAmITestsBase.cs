using System.Linq;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Functional.Messages
{

    public class WhoAmITestsBase : IntegrationTestBase
    {
        public WhoAmITestsBase(StartupFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper) { }

        [Fact()]
        public async Task ExecuteFunctionAsync_When_Function_WhoAmi_OrganizationId_Ok()
        {
            var response = await CrmClient.ExecuteFunctionAsync<WhoAmIResponse>(new WhoAmIRequest());

            response.OrganizationId.Should().Be(Setup.OrganizationId);
            response.BusinessUnitId.Should().NotBeEmpty();
            response.UserId.Should().NotBeEmpty();
        }

        [Fact()]
        public void GetMyCrmUserId_Ok()
        {
            var userId = CrmClient.GetMyCrmUserId();

            userId.Should().NotBeEmpty();
        }

        [Fact()]
        public async Task When_Impersonated_Then_GetMyCrmUserId_Is_NOT_ImpersonatedUserId()
        {
            var userId = CrmClient.GetMyCrmUserId();

            var searchOptions = new QueryOptions("systemuserid")
                .Top(1)
                .Filter("fullname eq 'SYSTEM'");

            var collection = await CrmClient.RetrieveMultipleAsync("systemuser", searchOptions);
            
            var systemUser = collection.Entities.FirstOrDefault();

            CrmClient.CallerId = systemUser.Id;

            var impersonatedUserId = CrmClient.GetMyCrmUserId();

            impersonatedUserId.Should().NotBeEmpty();
            impersonatedUserId.Should().Be(userId);
        }
    }
}
