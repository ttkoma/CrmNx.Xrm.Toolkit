using System;
using System.Reflection;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class TestStartup : TestStartupBase
    {
        public override IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder builder)
        {
            return builder
                .AddUserSecrets(Assembly.GetExecutingAssembly()) //Microsoft.Extensions.Configuration.UserSecrets
                .SetBasePath(AppContext.BaseDirectory); // Microsoft.Extensions.Configuration.FileExtensions
        }

        public override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration,
            ITestOutputHelper outputHelper)
        {
            var setupInstance = new Setup();
            configuration.GetSection(nameof(Setup)).Bind(setupInstance);
            
            services.AddSingleton(setupInstance);

            services.AddCrmWebApiClient(s =>
            {
                s.ConnectionString = configuration.GetConnectionString("Crm");
                s.Username = configuration["CrmWebApiClient:Username"];
                s.Password = configuration["CrmWebApiClient:Password"];
            });
            
            return services;
        }
    }
}