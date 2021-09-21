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
    public class EntityTests
    {
        [Fact]
        public void ToEntityReference_When_Entity_LogicalName_Present_Then_EntityReference_LogicalName_Equal()
        {
            var entity = new Entity("account", SetupBase.EntityId);

            var entityRef = entity.ToEntityReference();

            entityRef.LogicalName.Should().Be("account");
        }

        [Fact]
        public void ToEntityReference_When_Entity_Id_Present_Then_EntityReference_Id_Equal()
        {
            var entity = new Entity("account", SetupBase.EntityId);

            var entityRef = entity.ToEntityReference();

            entityRef.Id.Should().Be(SetupBase.EntityId);
        }

        [Fact]
        public void
            ToEntityReference_When_Entity_Alternate_KeyValue_Present_Then_EntityReference_Alternate_KeyValue_Equal()
        {
            var entity = new Entity("account", "accountnumber", 777);

            var entityRef = entity.ToEntityReference();

            entityRef.Id.Should().Be(Guid.Empty);
            entityRef.KeyAttributes.Keys.Count.Should().Be(1);
            entityRef.KeyAttributes.ContainsKey("accountnumber").Should().BeTrue();
            entityRef.KeyAttributes["accountnumber"].Should().Be(777);
        }


        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_Int32_Then_Return_0()
        {
            var entity = new Entity("account");
            var value = entity.GetAttributeValue<int>("dummy");

            value.Should().Be(default);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_Int32_Then_Return_NullableInt()
        {
            var entity = new Entity("account");
            var value = entity.GetAttributeValue<int?>("dummy");

            value.HasValue.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void GetAttributeValue_When_Entity_Contains_Attribute_Int32_Then_Return_NullableInt()
        {
            var entity = new Entity("account")
            {
                ["dummyIntField"] = 145
            };

            var value = entity.GetAttributeValue<int?>("dummyIntField");

            value.HasValue.Should().BeTrue();
            value.Value.Should().Be(145);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_EntityReference_Then_Return_Null()
        {
            var entity = new Entity("account");

            var value = entity.GetAttributeValue<EntityReference>("dummyIntField");

            value.Should().BeNull();
        }

        [Fact]
        public async Task GetAttributeValue_When_Entity_Contains_Attribute_EntityReference_Then_Return_Valid()
        {
            const string jsonContent = @"
            {
                ""@odata.context"": ""https://local.host/demo/api/data/v8.2/$metadata#systemusers/$entity"",
                ""_createdby_value@Microsoft.Dynamics.CRM.lookuplogicalname"": ""systemuser"",
                ""_createdby_value"": ""00000000-0000-0000-0000-000000000001""
            }";

            var apiResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));
            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = await crmClient.RetrieveAsync("systemuser", Guid.Empty);

            var value = entity.GetAttributeValue<EntityReference>("createdby");

            value.Should().NotBeNull();
            value.Should().BeEquivalentTo(new EntityReference("systemuser", SetupBase.EntityId));
        }

        [Fact]
        public async Task GetAttributeValue_When_Entity_Contains_Attribute_Date_Then_Return_Date()
        {
            const string jsonContent = @"
            {
                ""modifiedby"": ""2020-09-15T12:00:00Z"",
                ""systemuserid"": ""00000000-0000-0000-0000-000000000001""
            }";

            var apiResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            var entity = await crmClient.RetrieveAsync("systemuser", SetupBase.EntityId);

            var value = entity.GetAttributeValue<DateTime>("modifiedby");

            value.Should().Be(new DateTime(2020, 9, 15, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task GetAttributeValue_When_Entity_Contains_Attribute_Date_Then_Return_NullableDate()
        {
            const string jsonContent = @"
            {
                ""modifiedby"": ""2020-09-15T12:00:00Z"",
                ""systemuserid"": ""00000000-0000-0000-0000-000000000001""
            }";

            var apiResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new MockedHttpMessageHandler(apiResponse));
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            var entity = await crmClient.RetrieveAsync("systemuser", SetupBase.EntityId);
            var value = entity.GetAttributeValue<DateTime?>("modifiedby");

            value.Should().Be(new DateTime(2020, 9, 15, 12, 0, 0, 0, DateTimeKind.Utc));
        }


        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_Date_Then_Return_MinValue()
        {
            var entity = new Entity("account");

            var value = entity.GetAttributeValue<DateTime>("datefield");

            value.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_NullableDate_Then_Return_Null()
        {
            var entity = new Entity("account");

            var value = entity.GetAttributeValue<DateTime?>("datefield");

            value.Should().BeNull();
        }

        [Fact]
        public void GetAttributeValue_When_Entity_Contains_Attribute_Int32_Then_Return_AsEnum()
        {
            var entity = new Entity("account")
            {
                ["statecode"] = 1
            };

            var value = entity.GetAttributeValue<Account.StateCodeEnum>("statecode");

            value.Should().Be(Account.StateCodeEnum.InActive);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_Contains_Attribute_Int64_Then_Return_AsEnum()
        {
            var entity = new Entity("account")
            {
                ["statecode"] = (long) 1
            };

            var value = entity.GetAttributeValue<Account.StateCodeEnum>("statecode");

            value.Should().Be(Account.StateCodeEnum.InActive);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_NullableEnum_Then_Return_Null()
        {
            var entity = new Entity("account");

            var value = entity.GetAttributeValue<Account.StateCodeEnum?>("dummyIntField");

            value.Should().BeNull();
        }

        [Fact]
        public void GetAttributeValue_When_Entity_Contains_Attribute_Int32_Then_Return_NullableEnum()
        {
            var entity = new Entity("account")
            {
                ["statecode"] = 1
            };

            var value = entity.GetAttributeValue<Account.StateCodeEnum?>("statecode");

            value.HasValue.Should().BeTrue();
            value.Value.Should().Be(Account.StateCodeEnum.InActive);
        }

        [Fact]
        public void ToEntity_When_T_Derived_From_Entity_Then_Return_New_Entity()
        {
            var entity = new Entity()
            {
                Id = SetupBase.EntityId,
                [Account.PropertyNames.AccountNumber] = 777,
                [Account.PropertyNames.StateCode] = Account.StateCodeEnum.InActive
            };

            var account = entity.ToEntity<Account>();

            account.LogicalName.Should().Be(Account.EntityLogicalName);
            account.AccountNumber.Should().Be(777);
            account.StateCode.Should().Be(Account.StateCodeEnum.InActive);
            account.Id.Should().Be(SetupBase.EntityId);
        }
        
        [Fact]
        public async Task Retrieve_Request_Have_HttpHeader_Prefer_Included_ODataAnnotations()
        {
            HttpRequestHeaders requestHeaders = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestHeaders = request.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            var entity = new EntityReference("contact", SetupBase.EntityId);

            await crmClient.RetrieveAsync(entity);

            requestHeaders.Contains("Prefer").Should().BeTrue();

            requestHeaders.GetValues("Prefer").Contains("odata.include-annotations=\"*\"").Should().BeTrue();
        }
    }
}