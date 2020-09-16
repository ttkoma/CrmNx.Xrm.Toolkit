using System;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Functional
{
    public class CrmWebApiClientCreateTests : IntegrationTestBase
    {
        public CrmWebApiClientCreateTests(StartupFixture startup, ITestOutputHelper outputHelper)
            : base(startup, outputHelper) { }

        [Fact()]
        public async Task CreateAsync_When_Empty_Entity_Then_ReturnId()
        {
            var entity = new Entity("contact");

            var id = await CrmClient.CreateAsync(entity);

            try
            {
                id.Should().NotBeEmpty();
            }
            finally
            {
                await CrmClient.DeleteAsync("contact", id);
            }
        }

        [Fact()]
        public async Task CreateAsync_When_Present_Id_Then_ReturnId()
        {
            var entity = new Entity("account")
            {
                Id = Setup.EntityId
            };

            Guid Id = await CrmClient.CreateAsync(entity);

            try
            {
                Id.Should().NotBeEmpty();
            }
            finally
            {
                await CrmClient.DeleteAsync("account", Id);
            }
        }

        [Fact()]
        public async Task CreateAsync_When_Present_PrimaryIdAttribute_Then_ReturnId()
        {
            var entity = new Entity("account");
            entity["accountid"] = Setup.EntityId;

            var id = await CrmClient.CreateAsync(entity);

            try
            {
                id.Should().NotBeEmpty();
            }
            finally
            {
                await CrmClient.DeleteAsync("account", id);
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_CustomerProperty_Then_Ok()
        {
            var conactId = await CrmClient.CreateAsync(new Entity("contact", Setup.EntityId));
            var contactRef = new EntityReference("contact", conactId);

            var opportunity = new Entity("opportunity");
            opportunity["customerid_contact"] = contactRef;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();
                var opportunityFormBd = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value, new QueryOptions("customerid"));
                var customerRef = opportunityFormBd.GetAttributeValue<EntityReference>("customerid");
                customerRef.Should().NotBeNull();
                customerRef.Should().BeEquivalentTo(contactRef);

            }
            finally
            {
                if (opportunityId.HasValue)
                {
                    await CrmClient.DeleteAsync("opportunity", opportunityId.Value);
                }

                await CrmClient.DeleteAsync("contact", conactId);
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_DateTimeProperty_Then_Ok()
        {
            var finaldecisiondate = DateTime.Now.Date.ToUniversalTime();

            var opportunity = new Entity("opportunity");
            opportunity["finaldecisiondate"] = finaldecisiondate;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                var opportunityFormBd = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value, new QueryOptions("finaldecisiondate"));
                var actualDate = opportunityFormBd.GetAttributeValue<DateTime>("finaldecisiondate");
                actualDate.Should().Be(finaldecisiondate);
            }
            finally
            {
                if (opportunityId.HasValue)
                {
                    await CrmClient.DeleteAsync("opportunity", opportunityId.Value);
                }
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_MoneyProperty_Then_Ok()
        {
            var budgetamount = 123.15d;

            var opportunity = new Entity("opportunity");
            opportunity["budgetamount"] = budgetamount;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                var opportunityFormBd = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value, new QueryOptions("budgetamount"));
                var actualBudged = opportunityFormBd.GetAttributeValue<double>("budgetamount");
                actualBudged.Should().Be(budgetamount);
            }
            finally
            {
                if (opportunityId.HasValue)
                {
                    await CrmClient.DeleteAsync("opportunity", opportunityId.Value);
                }
            }
        }
    }
}
