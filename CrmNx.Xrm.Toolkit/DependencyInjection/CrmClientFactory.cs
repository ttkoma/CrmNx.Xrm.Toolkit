using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrmNx.Xrm.Toolkit.DependencyInjection
{
    public class CrmClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CrmClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICrmClient Create => _serviceProvider.GetRequiredService<ICrmClient>();
    }
}
