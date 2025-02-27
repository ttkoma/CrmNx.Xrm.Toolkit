using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages;

public class RetrieveTimestampTests : IntegrationTest<TestStartup>
{
    public RetrieveTimestampTests(TestStartup fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task RetrieveTimestamp_Timestamp_IS_STRING()
    {
        var request = new RetrieveTimestampRequest();
        var response = await CrmClient.ExecuteAsync(request);

        response.Should().NotBeNull();
        response.Timestamp.Should().NotBeNullOrEmpty();
    }
}