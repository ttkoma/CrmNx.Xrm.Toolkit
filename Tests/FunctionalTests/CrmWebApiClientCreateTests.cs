using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class CrmWebApiClientCreateTests : IntegrationTest<TestStartup>
    {
        public CrmWebApiClientCreateTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

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
                Id = Crm.Toolkit.Testing.SetupBase.EntityId
            };

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

        [Fact()]
        public async Task CreateAsync_When_Present_PrimaryIdAttribute_Then_ReturnId()
        {
            var entity = new Entity("account")
            {
                ["accountid"] = Crm.Toolkit.Testing.SetupBase.EntityId
            };

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
            var contactId = await CrmClient.CreateAsync(new Entity("contact", Crm.Toolkit.Testing.SetupBase.EntityId));
            var contactRef = new EntityReference("contact", contactId);

            var opportunity = new Entity("opportunity");
            opportunity["customerid_contact"] = contactRef;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();
                var opportunityFormDb = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value,
                    new QueryOptions("customerid"));
                var customerRef = opportunityFormDb.GetAttributeValue<EntityReference>("customerid");
                customerRef.Should().NotBeNull();
                customerRef.Should().BeEquivalentTo(contactRef);
            }
            finally
            {
                if (opportunityId.HasValue)
                {
                    await CrmClient.DeleteAsync("opportunity", opportunityId.Value);
                }

                await CrmClient.DeleteAsync("contact", contactId);
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_DateTime_Property_Then_Ok()
        {
            var finaldecisiondate = DateTime.Now.Date.ToUniversalTime();

            var opportunity = new Entity("opportunity");
            opportunity["finaldecisiondate"] = finaldecisiondate;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                var opportunityFormDb = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value,
                    new QueryOptions("finaldecisiondate"));
                var actualDate = opportunityFormDb.GetAttributeValue<DateTime>("finaldecisiondate");
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
        public async Task CreateAsync_When_Present_Money_Property_Then_Ok()
        {
            var budgetamount = 123.15d;

            var opportunity = new Entity("opportunity");
            opportunity["budgetamount"] = budgetamount;

            Guid? opportunityId = null;
            try
            {
                opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                var opportunityFormDb = await CrmClient.RetrieveAsync("opportunity", opportunityId.Value,
                    new QueryOptions("budgetamount"));
                var actualBudged = opportunityFormDb.GetAttributeValue<double>("budgetamount");
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

        [Fact]
        public async Task CreateAsync_When_Present_DateOnly_Property_Then_Ok()
        {
            var birthdate = new DateTime(2020, 09, 17);

            var contact = new Entity("contact")
            {
                ["birthdate"] = birthdate
            };

            Guid entityId = default;
            try
            {
                entityId = await CrmClient.CreateAsync(contact);

                entityId.Should().NotBeEmpty();

                var contactFromDb = await CrmClient.RetrieveAsync("contact", entityId, new QueryOptions("birthdate"));
                var actualValue = contactFromDb.GetAttributeValue<DateTime>("birthdate");
                actualValue.Should().Be(birthdate);
            }
            finally
            {
                if (!Guid.Empty.Equals(entityId))
                {
                    await CrmClient.DeleteAsync("contact", entityId);
                }
            }
        }
    }
}