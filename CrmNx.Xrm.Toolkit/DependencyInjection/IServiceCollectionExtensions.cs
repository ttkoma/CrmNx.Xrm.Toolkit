using CrmNx.Xrm.Toolkit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

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
        public static IHttpClientBuilder AddCrmWebApiClient(this IServiceCollection services, string connectionString)
        {
            return services.AddCrmWebApiClient<CrmWebApiClient, WebApiMetadata>(connectionString);
        }

        public static IHttpClientBuilder AddCrmWebApiClient(this IServiceCollection services,
            Action<CrmClientSettings> configureOptions)
        {
            return services.AddCrmWebApiClient<CrmWebApiClient, WebApiMetadata>(configureOptions);
        }

        public static IHttpClientBuilder AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(
            this IServiceCollection services, string connectionString)
            where TCrmWebApiClient : class, ICrmWebApiClient
            where TWebApiMetadataService : class, IWebApiMetadataService
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    $"Connection string with name '{CrmClientSettings.DefaultConnectionStringName}' not found in configuration.");
            }

            return services.AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(options =>
            {
                options.ConnectionString = connectionString;
            });
        }

        public static IHttpClientBuilder AddCrmWebApiClient<TCrmWebApiClient, TWebApiMetadataService>(
            this IServiceCollection services,
            Action<CrmClientSettings> configureOptions)
            where TCrmWebApiClient : class, ICrmWebApiClient
            where TWebApiMetadataService : class, IWebApiMetadataService
        {
            var settings = new CrmClientSettings();
            configureOptions?.Invoke(settings);
            
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new ArgumentException("Invalid Crm ConnectionString");
            }
            
            IOptions<CrmClientSettings> options = Options.Create(settings);
            
            services.AddSingleton<IOptions<CrmClientSettings>>(options);
            services.AddSingleton<CrmClientFactory>();
            services.AddSingleton<IWebApiMetadataService, TWebApiMetadataService>();
            
            var httpClientBuilder = services.AddHttpClient<ICrmWebApiClient, TCrmWebApiClient>((client) =>
                {
                    client.BaseAddress = settings.BaseAddress;
                    client.Timeout = settings.Timeout;
                    
                    var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
                    acceptHeader.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                    acceptHeader.Parameters.Add(new NameValueHeaderValue("odata.streaming", "true"));

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(acceptHeader);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("OData-Version", "4.0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("OData-MaxVersion", "4.0");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        UseDefaultCredentials = settings.UseDefaultCredentials,
                        Credentials = Credentials(settings),
                        PreAuthenticate = true,
                        UseCookies = settings.UseCookies,
                    };
                    
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }

                    if (settings.IgnoreSSLErrors)
                    {
                        // TODO: Set ignoring diffirent System.Net.Security.SslPolicyErrors
                        // e.g. RemoteCertificateNameMismatch
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            return true;
                        };
                    }

                    return handler;
                });
            
            if (settings.HandlerLifetime is not null)
            {
                httpClientBuilder.SetHandlerLifetime(settings.HandlerLifetime.Value);
            }
            
            return httpClientBuilder;
        }


        private static ICredentials Credentials(CrmClientSettings settings)
        {
            if (settings.UseDefaultCredentials)
            {
                return CredentialCache.DefaultNetworkCredentials;
            }

            var creds = new CredentialCache
            {
                {
                    settings.BaseAddress, settings.AuthType,
                    new NetworkCredential(settings.Username, settings.Password, settings.Domain)
                }
            };

            return creds;
        }
    }
}