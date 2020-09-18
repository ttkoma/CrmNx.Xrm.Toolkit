using CrmNx.Xrm.Toolkit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Crm.Toolkit.Testing.Functional
{
    public class IntegrationTest<TTestStartup> : IClassFixture<TTestStartup>
        where TTestStartup : class
    {
        private ICrmWebApiClient _crmClient;
        private ServiceProvider _serviceProvider;

        protected ICrmWebApiClient CrmClient =>
            _crmClient ??= ServiceProvider.GetRequiredService<ICrmWebApiClient>();

        protected ServiceProvider ServiceProvider =>
            _serviceProvider ??= ServicesCollection.BuildServiceProvider(ServiceProviderOptions);

        protected readonly IServiceCollection ServicesCollection;
        protected readonly ServiceProviderOptions ServiceProviderOptions = new ServiceProviderOptions();

        protected IntegrationTest(TestStartupBase fixture, ITestOutputHelper outputHelper)
        {
            var configuration = fixture.ConfigureConfiguration(GetConfigurationBuilder()).Build();

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services = ConfigureLogging(services, outputHelper);

            services = fixture.ConfigureServices(services, configuration, outputHelper);

            ServicesCollection = services;
        }

        private IServiceCollection ConfigureLogging(IServiceCollection services, ITestOutputHelper outputHelper)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.TestOutput(outputHelper, Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger()
                .ForContext<TTestStartup>();

            services.AddLogging(builder => { builder.AddSerilog(logger, true); });

            return services;
        }

        private IConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false) // Microsoft.Extensions.Configuration.Json
                .AddEnvironmentVariables(); //Microsoft.Extensions.Configuration.EnvironmentVariables
        }
    }
}