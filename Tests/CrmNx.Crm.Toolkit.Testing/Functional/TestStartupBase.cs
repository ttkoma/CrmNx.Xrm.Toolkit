using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CrmNx.Crm.Toolkit.Testing.Functional
{
    public abstract class TestStartupBase
    {
        /// <summary>
        /// Configure additional configuration providers
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public abstract IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder builder);

        /// <summary>
        /// Register additional services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="outputHelper"></param>
        /// <returns></returns>
        public abstract IServiceCollection ConfigureServices(IServiceCollection services,
            IConfiguration configuration, ITestOutputHelper outputHelper);
    }
}