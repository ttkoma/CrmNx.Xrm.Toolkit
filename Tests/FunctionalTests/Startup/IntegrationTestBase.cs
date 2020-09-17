using CrmNx.Xrm.Toolkit.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class IntegrationTestBase : IClassFixture<StartupFixture>
    {
        private ICrmWebApiClient _crmClient;
        private readonly StartupFixture _fixture;

        protected ICrmWebApiClient CrmClient
        {
            get
            {
                if (_crmClient == null)
                {
                    _crmClient = _fixture.ServiceProvider.GetRequiredService<ICrmWebApiClient>();
                }

                return _crmClient;
            }
        }

        public IntegrationTestBase(StartupFixture fixture, ITestOutputHelper outputHelper)
        {
            fixture.OutputHelper = outputHelper;
            _fixture = fixture;

            ConfigureServices(fixture.ServiceCollections, fixture.Configuration);
        }

        protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCrmWebApiClient(options =>
            {
                // From appsettings.json
                options.ConnectionString = configuration.GetConnectionString("Crm");

                options.Username = configuration["CrmWebApiClient:Username"];
                options.Password = configuration["CrmWebApiClient:Password"];
            });
        }
    }
}