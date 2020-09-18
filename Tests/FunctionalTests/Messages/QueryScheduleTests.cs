using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages
{
    public class QueryScheduleTests : IntegrationTest<TestStartup>
    {
        public QueryScheduleTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact()]
        public async Task Execute_QuerySchedule_When_Resource_Is_CurrentUser_Then_ResultOk()
        {
            var request = new QueryScheduleRequest()
            {
                ResourceId = CrmClient.GetMyCrmUserId(),
                Start = System.DateTime.Now.Date,
                End = System.DateTime.Now.Date.AddDays(1).AddSeconds(-1),
                TimeCodes = new TimeCode[] {TimeCode.Available}
            };

            var response = await CrmClient.ExecuteAsync<QueryScheduleResponse>(request);

            response.TimeInfos.Should().NotBeNull();
            response.TimeInfos.Should().NotBeEmpty();
        }
    }
}