using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing;
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
                if (!Guid.Empty.Equals(id))
                    await CrmClient.DeleteAsync(new EntityReference("contact", id));
            }
        }

        [Fact()]
        public async Task CreateAsync_When_Present_Id_Then_ReturnId()
        {
            var entity = new Entity("account")
            {
                Id = SetupBase.EntityId
            };

            var id = await CrmClient.CreateAsync(entity);

            try
            {
                id.Should().NotBeEmpty();
            }
            finally
            {
                if (!Guid.Empty.Equals(id))
                    await CrmClient.DeleteAsync(new EntityReference("account", id));
            }
        }

        [Fact()]
        public async Task CreateAsync_When_Present_PrimaryIdAttribute_Then_ReturnId()
        {
            var entity = new Entity("account")
            {
                ["accountid"] = SetupBase.EntityId
            };

            var id = await CrmClient.CreateAsync(entity);

            try
            {
                id.Should().NotBeEmpty();
            }
            finally
            {
                if (!Guid.Empty.Equals(id))
                    await CrmClient.DeleteAsync(new EntityReference("account", id));
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_CustomerProperty_Then_Ok()
        {
            var contactId = await CrmClient.CreateAsync(new Entity("contact", SetupBase.EntityId));
            var contactRef = new EntityReference("contact", contactId);

            var opportunity = new Entity("opportunity")
            {
                ["customerid_contact"] = contactRef
            };


            EntityReference opportunityRef = default;
            try
            {
                var opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                opportunityRef = new EntityReference("opportunity", opportunityId);

                var opportunityFormDb =
                    await CrmClient.RetrieveAsync(opportunityRef, QueryOptions.Select("customerid"));
                var customerRef = opportunityFormDb.GetAttributeValue<EntityReference>("customerid");
                customerRef.Should().NotBeNull();
                customerRef.Should().BeEquivalentTo(contactRef);
            }
            finally
            {
                if (opportunityRef != null)
                {
                    await CrmClient.DeleteAsync(opportunityRef);
                }

                await CrmClient.DeleteAsync(contactRef);
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_DateTime_Property_Then_Ok()
        {
            const string propertyName = "finaldecisiondate";
            var propertyValue = DateTime.Now.Date.ToUniversalTime();

            var opportunity = new Entity("opportunity");
            opportunity.SetAttributeValue(propertyName, propertyValue);

            EntityReference opportunityRef = default;
            try
            {
                var opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                opportunityRef = new EntityReference("opportunity", opportunityId);
                var opportunityFormDb =
                    await CrmClient.RetrieveAsync(opportunityRef, QueryOptions.Select(propertyName));
                var actualDate = opportunityFormDb.GetAttributeValue<DateTime>(propertyName);
                actualDate.Should().Be(propertyValue);
            }
            finally
            {
                if (opportunityRef != null)
                {
                    await CrmClient.DeleteAsync(opportunityRef);
                }
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_Money_Property_Then_Ok()
        {
            const string propertyName = "budgetamount";
            const double propertyValue = 123.15d;

            var opportunity = new Entity("opportunity")
            {
                [propertyName] = propertyValue
            };

            EntityReference opportunityRef = null;
            try
            {
                var opportunityId = await CrmClient.CreateAsync(opportunity);
                opportunityId.Should().NotBeEmpty();

                opportunityRef = new EntityReference("opportunity", opportunityId);

                var opportunityFormDb =
                    await CrmClient.RetrieveAsync(opportunityRef, QueryOptions.Select(propertyName));
                var actualBudged = opportunityFormDb.GetAttributeValue<double>(propertyName);
                actualBudged.Should().Be(propertyValue);
            }
            finally
            {
                if (opportunityRef != null)
                {
                    await CrmClient.DeleteAsync(opportunityRef);
                }
            }
        }

        [Fact]
        public async Task CreateAsync_When_Present_DateOnly_Property_Then_Ok()
        {
            const string propertyName = "birthdate";
            var propertyValue = new DateTime(2020, 09, 17);

            var contact = new Entity("contact");
            contact.SetAttributeValue(propertyName, propertyValue);

            EntityReference contactRef = default;
            try
            {
                var id = await CrmClient.CreateAsync(contact);
                id.Should().NotBeEmpty();

                contactRef = new EntityReference("contact", id);
                var contactFromDb = await CrmClient.RetrieveAsync(contactRef, QueryOptions.Select(propertyName));
                var actualValue = contactFromDb.GetAttributeValue<DateTime>(propertyName);
                actualValue.Should().Be(propertyValue);
            }
            finally
            {
                if (contactRef != null)
                {
                    await CrmClient.DeleteAsync(contactRef);
                }
            }
        }
    }
}