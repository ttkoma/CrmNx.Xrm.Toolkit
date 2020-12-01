using CrmNx.Xrm.Identity.AuthHandlers;
using CrmNx.Xrm.Identity.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrmNx.Xrm.Identity.DependencyInjection
{
    /// <summary>
    /// Добавляет Claims с информацией на основе учётной записи пользователя CRM
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет Аутенфтификацию Windows и авторизацию пользователя на основе утверждений из Crm
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrmAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(IISServerDefaults.AuthenticationScheme);

            services.AddTransient<IClaimsTransformation, CrmClaimsTransformer>();

            services.AddSingleton<IAuthorizationHandler, SystemUserHandler>();

            return services;
        }

        public static IServiceCollection AddCrmAuthenticationDevelopment(this IServiceCollection services,
                Action<DeveloperCrmClaimsTransformOptions> configure)
        {
            services.AddAuthentication(IISServerDefaults.AuthenticationScheme);
            services.AddTransient<IClaimsTransformation, DeveloperCrmClaimsTransformer>();

            services.Configure<DeveloperCrmClaimsTransformOptions>(configure);

            services.AddSingleton<IAuthorizationHandler, SystemUserHandler>();
            return services;
        }

        public static IServiceCollection AddCrmAuthenticationDevelopment(this IServiceCollection services,
        string userName)
        {
            services.AddAuthentication(IISServerDefaults.AuthenticationScheme);
            services.AddTransient<IClaimsTransformation, DeveloperCrmClaimsTransformer>();

            services.Configure<DeveloperCrmClaimsTransformOptions>(options => {
                options.UserDomainName = userName;
            });

            services.AddSingleton<IAuthorizationHandler, SystemUserHandler>();
            return services;
        }

        /// <summary>
        /// Добавляет политики авторизации на основе Crm (<see cref="CrmPolicies"/>).
        /// </summary>
        /// <param name="options">Параметры авторизации</param>
        /// <returns></returns>
        /// <remarks>
        /// Пример использования: [<see cref="AuthorizeAttribute" /> (Policy = <see cref="CrmPolicies.SystemUser"/>)]
        /// </remarks>
        public static AuthorizationOptions AddCrmPolicies(this AuthorizationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AddPolicy(CrmPolicies.SystemUser, policy =>
                {
                    policy.AddRequirements(new SystemUserRequirement());
                }
            );

            return options;
        }
    }
}