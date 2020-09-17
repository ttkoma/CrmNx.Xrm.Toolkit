using System;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class CrmWebApiClientRetrieveTests : IntegrationTestBase
    {
        public CrmWebApiClientRetrieveTests(StartupFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper) { }

        [Fact()]
        public async Task RetrieveAsync_When_Select_Only_Id_Then_Ok()
        {
            var options = new QueryOptions("organizationid");
            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId, options);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(Setup.OrganizationId);
            entity.LogicalName.Should().Be("organization");
        }

        [Fact()]
        public async Task RetrieveAsync_When_ColumnsSet_IsNull_Then_Only_Id_Returned()
        {
            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(Setup.OrganizationId);
            entity.LogicalName.Should().Be("organization");
            entity.Attributes.Count.Should().BeLessOrEqualTo(1);
            entity.Attributes.ContainsKey("organizationid").Should().BeTrue();

        }

        [Fact()]
        public async Task RetrieveAsync_When_Select_AllFields_Then_Ok()
        {
            var options = new QueryOptions()
                .Select(new ColumnSet { AllColumns = true });

            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId, options);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(Setup.OrganizationId);
            entity.LogicalName.Should().Be("organization");
            entity.Attributes.Count.Should().BeGreaterThan(1);
        }

        [Fact()]
        public async Task RetrieveAsync_When_Select_ModifiedBy_Then_EntityReference_Correct()
        {

            var options = new QueryOptions("modifiedby");

            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId, options);

            entity.Should().NotBeNull();
            entity.GetAttributeValue<EntityReference>("modifiedby").Should().NotBeNull();
            entity.GetAttributeValue<EntityReference>("modifiedby").Id.Should().NotBeEmpty();
            entity.GetAttributeValue<EntityReference>("modifiedby").LogicalName.Should().Be("systemuser");
        }

        [Fact()]
        public async Task RetrieveAsync_When_Expand_Then_Retrieved_NestedEntity_Correct()
        {
            var houseFias = Setup.HouseFiasGuid;
            var houseFields = new ColumnSet("gm_name", "gm_shortname", "gm_scrname", "gm_basicinformation",
            "gm_managementcompanycomment", "gm_fiasguid", "gm_isprivatedistrict",
            "gm_connected", "gm_networktype", "gm_partnersid");

            var options = new QueryOptions()
                .Select(houseFields)
                .Expand("gm_partnersid", "gm_partnersname","statecode","gm_partnersid");

            var houseReference = new EntityReference("gm_house", "gm_fiasguid", houseFias);

            var entity = await CrmClient.RetrieveAsync(houseReference, options);

            entity.Should().NotBeNull();
            entity.GetAttributeValue<Entity>("gm_partnersid").Should().NotBeNull();

        }

        [Fact]
        public async Task RetireveAsync_When_DateOnly_Attribute_Then_Correct_DateValue()
        {
            var accountReference = new EntityReference("contact", Setup.ContactId);
            var accountFields = new QueryOptions("birthdate");

            var entity = await CrmClient.RetrieveAsync(accountReference, accountFields);

            entity.Should().NotBeNull();

            entity.GetAttributeValue<DateTime>("birthdate")
                .Date.Should().Be(new DateTime(1986, 12, 21));

            entity.GetAttributeValue<DateTime>("birthdate")
                .TimeOfDay.Should().Be(new TimeSpan());

            entity.GetAttributeValue<DateTime>("birthdate")
                .Kind.Should().Be(DateTimeKind.Unspecified);
        }
    }
}