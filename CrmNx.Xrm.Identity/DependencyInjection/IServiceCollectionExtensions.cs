using System;
using CrmNx.Xrm.Identity;
using CrmNx.Xrm.Identity.AuthHandlers;
using CrmNx.Xrm.Identity.Internal;
using CrmNx.Xrm.Identity.Requirements;
using CrmNx.Xrm.Toolkit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CrmNx.DependencyInjection
{
    /// <summary>
    /// Добавляет Claims с информацией на основе учётной записи пользователя CRM
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Crm Claims
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrmClaims<TCrmClaimsProvider>(this IServiceCollection services)
            where TCrmClaimsProvider : class, ICrmClaimsProvider
        {
            services.AddTransient<ICrmClaimsProvider, TCrmClaimsProvider>();
            services.AddTransient<IClaimsTransformation, CrmClaimsTransformer>();

            return services;
        }

        /// <summary>
        /// Add Crm Claims
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrmClaims(this IServiceCollection services)
        {
            services.AddTransient<ICrmClaimsProvider, CrmClaimsByUserNameProvider>();
            services.AddTransient<IClaimsTransformation, CrmClaimsTransformer>();

            return services;
        }

        /// <summary>
        /// Add Crm Claims
        /// </summary>
        /// <param name="services">ServicesCollection</param>
        /// <param name="userName">Impersonated crm username (DOMAIN\\User.Name) for testing only</param>
        /// <returns></returns>
        public static IServiceCollection AddCrmClaims(this IServiceCollection services, string userName)
        {
            services.AddTransient<ICrmClaimsProvider>(sp =>
                new CrmClaimsByUserNameProvider(sp.GetRequiredService<ICrmWebApiClient>())
                {
                    ImpersonatedUserName = userName
                });

            services.AddTransient<IClaimsTransformation, CrmClaimsTransformer>();

            return services;
        }

        public static IServiceCollection AddCrmAuthorization<TAuthorizationHandler>(this IServiceCollection services,
            Action<AuthorizationOptions> configureOptions)
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            services.AddSingleton<IAuthorizationHandler, TAuthorizationHandler>();

            services.AddAuthorizationCore(configureOptions);

            return services;
        }

        /// <summary>
        /// Add Authorization Handler and configure <see cref="CrmPolicies.ActiveSystemUser"/> policy with requirements.
        /// </summary>
        /// <param name="services">Service collection <see cref="IServiceCollection"/></param>
        /// <returns></returns>
        /// <remarks>
        /// Usage: [<see cref="AuthorizeAttribute" /> (Policy = <see cref="CrmPolicies.ActiveSystemUser"/>)]
        /// </remarks>
        public static IServiceCollection AddCrmAuthorization(this IServiceCollection services)
        {
            AddCrmAuthorization<SystemUserAuthHandler>(services, options => { options.AddActiveSystemUserPolicy(); });

            return services;
        }

        /// <summary>
        /// Add Policy validated claims with SystemUserId Crm (<see cref="CrmPolicies"/>).
        /// </summary>
        /// <param name="options">Authorization options</param>
        /// <returns></returns>
        /// <remarks>
        /// Usage: [<see cref="AuthorizeAttribute" /> (Policy = <see cref="CrmPolicies.ActiveSystemUser"/>)]
        /// </remarks>
        public static AuthorizationOptions AddActiveSystemUserPolicy(this AuthorizationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AddPolicy(CrmPolicies.ActiveSystemUser,
                policy => { policy.AddRequirements(new SystemUserRequirement()); }
            );

            return options;
        }
    }
}