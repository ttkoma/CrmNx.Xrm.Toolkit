using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages
{
    public class GetAllTimeZonesWithDisplayNameTests: IntegrationTest<TestStartup>
    {
        public GetAllTimeZonesWithDisplayNameTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task When_LocaleId_Is_Equal_0_ThrowException()
        {
            var localeId = 0;

            Func<Task> invoker = async () => { await CrmClient.GetAllTimeZonesWithDisplayNameAsync(localeId, CancellationToken.None); };


            var assert = await invoker.Should().ThrowAsync<WebApiException>()
                .WithMessage("An unexpected error occurred.");
        }
        
        [Fact]
        public async Task When_LocaleId_Is_ruRU_Then_UserName_IsOK()
        {
            var localeId = 1049;

            var collection = await CrmClient.GetAllTimeZonesWithDisplayNameAsync(localeId, CancellationToken.None);

            collection?.Entities?.Should().NotBeNull();

            var timezone = collection?.Entities?.FirstOrDefault(x => x.GetAttributeValue<int>("timezonecode") == 0);
            timezone.Should().NotBeNull();
            timezone.GetAttributeValue<string>("userinterfacename")
                .Should().Be("(GMT-12:00) Межд. линия перемены дат");
        }
        
        [Fact]
        public async Task When_LocaleId_Is_EN_Then_UserName_IsOK()
        {
            var localeId = 1033;

            var collection = await CrmClient.GetAllTimeZonesWithDisplayNameAsync(localeId, CancellationToken.None);

            collection?.Entities?.Should().NotBeNull();

            var timezone = collection?.Entities?.FirstOrDefault(x => x.GetAttributeValue<int>("timezonecode") == 0);
            timezone.Should().NotBeNull();
            timezone.GetAttributeValue<string>("userinterfacename")
                .Should().Be("(GMT-12:00) International Date Line West");
        }
    }
}