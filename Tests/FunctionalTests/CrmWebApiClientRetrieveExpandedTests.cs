using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class CrmWebApiClientRetrieveExpandedTest : IntegrationTest<TestStartup>
    {
        public CrmWebApiClientRetrieveExpandedTest(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        private static class TestSetup
        {
            /// <summary>
            /// Идентификатор слота с заполненной заявкой
            /// </summary>
            internal static Guid SlotIdForTests => new Guid("59510b56-e1b1-494c-9d13-0000155b6a70");

            /// <summary>
            /// Идентификатор заявки связанной со слотом
            /// </summary>
            internal static Guid IncidentIdAssociatedWithTestedSlot => new Guid("1b67db95-ac7f-ea11-aadd-005056b410d8");
        }

        [Fact]
        public async Task When_ExpandIncidentBySlotId_Returned_IncidentId()
        {
            // Подготовка
            var slotReference = new EntityReference("serviceappointment", TestSetup.SlotIdForTests);
            var query = new QueryOptions().Expand("sd_incidentid");

            // Выполнение
            var slotEntity = await CrmClient.RetrieveAsync(slotReference, query);

            // Проверки
            slotEntity.Should().NotBeNull();
            var incidentEntity = slotEntity["sd_incidentid_serviceappointment"] as Entity;

            incidentEntity.Should().NotBeNull();
            incidentEntity.LogicalName.Should().Be("incident");
            incidentEntity.Id.Should().Be(TestSetup.IncidentIdAssociatedWithTestedSlot);
            incidentEntity.GetAttributeValue<Guid>("incidentid").Should()
                .Be(TestSetup.IncidentIdAssociatedWithTestedSlot);
        }
    }
}