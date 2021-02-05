using System.Linq;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages
{
    public class WhoAmITests : IntegrationTest<TestStartup>
    {
        private readonly Setup Setup;
        public WhoAmITests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
            Setup = ServiceProvider.GetRequiredService<Setup>();
        }

        [Fact()]
        public async Task ExecuteFunctionAsync_When_Function_WhoAmi_OrganizationId_Ok()
        {
            var response = await CrmClient.ExecuteAsync(new WhoAmIRequest());

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

            var searchOptions = QueryOptions
                .Select("systemuserid")
                .Top(1)
                .Filter("fullname eq 'SYSTEM'");

            var collection = await CrmClient.RetrieveMultipleAsync("systemuser", searchOptions);

            var systemUser = collection.Entities.FirstOrDefault();

            if (systemUser != null) CrmClient.CallerId = systemUser.Id;

            var impersonatedUserId = CrmClient.GetMyCrmUserId();

            impersonatedUserId.Should().NotBeEmpty();
            impersonatedUserId.Should().Be(userId);
        }
    }
}