using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Crm.Toolkit.Testing.ProxyClasses;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class CrmWebApiClientRetrieveTests : IntegrationTest<TestStartup>
    {
        private readonly Setup Setup;
        public CrmWebApiClientRetrieveTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
            Setup = ServiceProvider.GetRequiredService<Setup>();
        }

        [Fact()]
        public async Task RetrieveAsync_When_Select_Only_Id_Then_Ok()
        {
            var options = QueryOptions.Select("organizationid");
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
                .Select(new ColumnSet {AllColumns = true});

            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId, options);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(Setup.OrganizationId);
            entity.LogicalName.Should().Be("organization");
            entity.Attributes.Count.Should().BeGreaterThan(1);
        }

        [Fact()]
        public async Task RetrieveAsync_When_Select_ModifiedBy_Then_EntityReference_Correct()
        {
            var options = QueryOptions.Select("modifiedby");

            var entity = await CrmClient.RetrieveAsync("organization", Setup.OrganizationId, options);

            entity.Should().NotBeNull();
            entity.GetAttributeValue<EntityReference>("modifiedby").Should().NotBeNull();
            entity.GetAttributeValue<EntityReference>("modifiedby").Id.Should().NotBeEmpty();
            entity.GetAttributeValue<EntityReference>("modifiedby").LogicalName.Should().Be("systemuser");
        }

        [Fact()]
        public async Task RetrieveAsync_When_Expand_Then_Retrieved_NestedEntity_Correct()
        {
            var account = new Account()
            {
                ["primarycontactid"] = new Entity("contact")
                {
                    ["firstname"] = "[Test] First Name",
                    ["lastname"] = "[Test] Last Name",
                }
            };

            var accountId = await CrmClient.CreateAsync(account);

            var accountFiled = new[]
            {
                "*" // Only not NULL attributes
            };

            var contactFiled = new[]
            {
                "ownerid", "statecode", "firstname", "fullname", "birthdate"
            };
            
            var options = QueryOptions
                .Select(accountFiled)
                .Expand("primarycontactid", contactFiled);

            EntityReference accountReference = Account.Reference(accountId);
            EntityReference contactRef = null;
            
            try
            {
                var complexAccountEntity = await CrmClient.RetrieveAsync(accountReference, options);

                complexAccountEntity.Should().NotBeNull();
                var contactEntity = complexAccountEntity.GetAttributeValue<Entity>("primarycontactid");

                contactEntity
                    .Should().NotBeNull();
                contactEntity.GetAttributeValue<string>("firstname")
                    .Should().Be("[Test] First Name");

                contactEntity.Id
                    .Should().NotBeEmpty();
                
                contactRef = contactEntity.ToEntityReference();
            }
            finally
            {
                await CrmClient.DeleteAsync(accountReference);

                if (contactRef != null)
                    await CrmClient.DeleteAsync(contactRef);
            }
            
        }

        [Fact]
        public async Task RetrieveAsync_When_DateOnly_Attribute_Then_Correct_DateValue()
        {
            var contact = new Entity("contact")
            {
                ["birthdate"] = new DateTime(1986, 12, 21, 00, 00, 00)
            };

            var id = await CrmClient.CreateAsync(contact);
            
            var contactReference = new EntityReference("contact", id);
            var contactFields = QueryOptions.Select("birthdate");

            var entity = await CrmClient.RetrieveAsync(contactReference, contactFields);

            try
            {
                entity.Should().NotBeNull();

                var birthDate = entity.GetAttributeValue<DateTime>("birthdate");
                birthDate.Date.Should().Be(new DateTime(1986, 12, 21));

                birthDate.TimeOfDay.Should().Be(new TimeSpan());

                birthDate.Kind.Should().Be(DateTimeKind.Unspecified);
            } 
            finally
            {
                if (!Guid.Empty.Equals(id))
                    await CrmClient.DeleteAsync(new EntityReference("contact", id));
            }
        }
    }
}