using System;
using FluentAssertions;
using TestFramework;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests
{
    public class EntityTests
    {
        [Fact]
        public void ToEntityReference_When_Entity_LogicalName_Present_Then_EntityReference_LogicalName_Equal()
        {
            var entity = new Entity("account", Setup.EntityId);

            var entityRef = entity.ToEntityReference();

            entityRef.LogicalName.Should().Be("account");
        }

        [Fact]
        public void ToEntityReference_When_Entity_Id_Present_Then_EntityReference_Id_Equal()
        {
            var entity = new Entity("account", Setup.EntityId);

            var entityRef = entity.ToEntityReference();

            entityRef.Id.Should().Be(Setup.EntityId);
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
        public void GetAttributeValue_When_Entity_NotContains_Attribute_Then_Return_Default_For_Type()
        {
            var entity = new Entity("account");
            var value = entity.GetAttributeValue<int>("dummy");

            value.Should().Be(default);
        }

        [Fact]
        public void GetAttributeValue_When_Entity_NotContains_Attribute_Then_Return_Null_For_Nullable_Type()
        {
            var entity = new Entity("account");
            var value = entity.GetAttributeValue<int?>("dummy");

            value.Should().BeNull();
        }
    }
}