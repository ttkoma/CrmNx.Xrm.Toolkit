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
            return services
                .AddCrmWebApiClient(options =>
                {
                    options.ConnectionString = configuration.GetConnectionString("Crm");
                    options.Username = configuration["CrmWebApiClient:Username"];
                    options.Password = configuration["CrmWebApiClient:Password"];
                });
        }
    }
}