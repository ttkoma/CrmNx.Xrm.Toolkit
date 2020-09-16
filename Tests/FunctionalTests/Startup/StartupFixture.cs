using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class StartupFixture
    {
        private ServiceProvider _serviceProvider;

        public ServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    _serviceProvider = ConfigureLogging(ServiceCollections).BuildServiceProvider();
                }

                return _serviceProvider;
            }
        }

        public ServiceCollection ServiceCollections { get; private set; }

        public ITestOutputHelper OutputHelper { get; set; }

        public IConfiguration Configuration { get; private set; }

        public StartupFixture()
        {
            Configuration = GetIConfigurationRoot();

            ServiceCollections = new ServiceCollection();
        }

        private IServiceCollection ConfigureLogging(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.TestOutput(OutputHelper, Serilog.Events.LogEventLevel.Verbose)
                        .CreateLogger()
                        .ForContext<IntegrationTestBase>();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger, true);
            });

            return services;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Microsoft.Extensions.Configuration.FileExtensions
                .AddJsonFile("appsettings.json", optional: false) // Microsoft.Extensions.Configuration.Json
                .AddUserSecrets(Assembly.GetExecutingAssembly()) //Microsoft.Extensions.Configuration.UserSecrets
                .AddEnvironmentVariables() //Microsoft.Extensions.Configuration.EnvironmentVariables
                .Build();
        }
    }
}
