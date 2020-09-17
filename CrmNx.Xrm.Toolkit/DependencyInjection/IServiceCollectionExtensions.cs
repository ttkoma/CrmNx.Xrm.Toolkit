using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CrmNx.Xrm.Toolkit.DependencyInjection
{
    public static class CrmWebApiClientServiceCollectionExtensions
    {
        /// <summary>
        /// Add CrmWebApiClient to IoC
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="connectionString">Crm connectionString</param>
        /// <returns></returns>
        public static IServiceCollection AddCrmWebApiClient(this IServiceCollection services, string connectionString)
        {
            return services.AddCrmWebApiClient<CrmWebApiClient, WebApiMetadata>(connectionString);
        }

        public static IServiceCollection AddCrmWebApiClient(this IServiceCollection services,
            Action<CrmClientOptions> configureOptions)
        {
            return services.AddCrmWebApiClient<CrmWebApiClient, WebApiMetadata>(configureOptions);
        }

        public static IServiceCollection AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(
            this IServiceCollection services, string connectionString)
            where TCrmWebApiClient : class, ICrmWebApiClient
            where TWebApiMetadataService : class, IWebApiMetadataService
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    $"Connection string with name '{CrmClientOptions.DefaultConnectionStringName}' not found in configuration.");
            }

            services.AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(options =>
            {
                options.HandlerLifetime = TimeSpan.FromMinutes(9);
                options.ConnectionString = connectionString;
            });

            return services;
        }

        public static IServiceCollection AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(
            this IServiceCollection services,
            Action<CrmClientOptions> configureOptions)
            where TCrmWebApiClient : class, ICrmWebApiClient
            where TWebApiMetadataService : class, IWebApiMetadataService
        {
            var options = new CrmClientOptions();

            configureOptions?.Invoke(options);

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentException("Invalid Crm ConnectionString");
            }

            var httpClientBuilder = services.AddHttpClient<ICrmWebApiClient, TCrmWebApiClient>()
                .ConfigureHttpClient(client =>
                {
                    var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
                    acceptHeader.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                    acceptHeader.Parameters.Add(new NameValueHeaderValue("odata.streaming", "true"));

                    client.BaseAddress = options.BaseAddress;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(acceptHeader);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("OData-Version", "4.0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("OData-MaxVersion", "4.0");

                    //client.Timeout = TimeSpan.FromSeconds(30);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        UseDefaultCredentials = options.UseDefaultCredentials,
                        Credentials = Credentials(options),
                        PreAuthenticate = true,
                    };

                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }

                    if (options.IgnoreSSLErrors)
                    {
                        // TODO: Set ignoring difirent System.Net.Security.SslPolicyErrors
                        // e.g. RemoteCertificateNameMismatch
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            return true;
                        };
                    }

                    return handler;
                });

            if (options.HandlerLifetime != default)
            {
                httpClientBuilder.SetHandlerLifetime(options.HandlerLifetime);
            }

            services.AddSingleton<CrmClientFactory>();
            services.AddSingleton<IWebApiMetadataService, TWebApiMetadataService>();
            return services;
        }


        private static ICredentials Credentials(CrmClientOptions options)
        {
            if (options.UseDefaultCredentials)
            {
                return CredentialCache.DefaultNetworkCredentials;
            }

            var creds = new CredentialCache
            {
                {
                    options.BaseAddress, options.AuthType,
                    new NetworkCredential(options.Username, options.Password, options.Domain)
                }
            };

            return creds;
        }
    }
}