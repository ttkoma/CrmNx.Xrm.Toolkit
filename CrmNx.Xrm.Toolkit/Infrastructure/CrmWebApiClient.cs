using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using CrmNx.Xrm.Toolkit.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class CrmWebApiClient : ICrmWebApiClient
    {
        protected readonly HttpClient HttpClient;
        private readonly ILogger<CrmWebApiClient> _logger;
        private readonly JsonSerializer _serializer;

        public IWebApiMetadataService WebApiMetadata { get; }

        private const int MaxPageSize = 250;

        private static JsonSerializerSettings _serializerSettingsDefault;
        private static string JsonMediaType = MediaTypeHeaderValue.Parse("application/json").MediaType;

        public static JsonSerializerSettings SerializerSettings =>
            _serializerSettingsDefault ??= new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };


        public CrmWebApiClient(HttpClient httpClient, IWebApiMetadataService webApiMetadata,
            ILogger<CrmWebApiClient> logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            WebApiMetadata = webApiMetadata ?? throw new ArgumentNullException(nameof(webApiMetadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SerializerSettings.Converters.Add(new EntityConverter(webApiMetadata));
            SerializerSettings.Converters.Add(new EntityCollectionConverter(webApiMetadata));
            SerializerSettings.Converters.Add(new EntityReferenceConverter(webApiMetadata));

            _serializer = JsonSerializer.Create(SerializerSettings);
        }

        /// <summary>
        /// Gets or sets the current caller Id.
        /// </summary>
        public Guid CallerId { get; set; }

        public virtual async Task<Guid> CreateAsync(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var collectionName = WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var json = JsonConvert.SerializeObject(entity, SerializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{collectionName}")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            using var httpResponse = await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, default)
                .ConfigureAwait(false);

            httpRequest.Dispose();

            Guid entityId = default;

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                if (httpResponse.Headers.TryGetValues("OData-EntityId", out var headersRaw))
                {
                    var headers = headersRaw as string[] ?? headersRaw.ToArray();

                    var rawValue = headers.First().Split("(").Last().Split(")").First();
                    if (!Guid.TryParse(rawValue, out entityId))
                    {
                        throw new WebApiException(
                            $"Error parsing response header 'OData-EntityId'. Value: {headers.First()}");
                    }
                }
                else
                {
                    throw new WebApiException("Response header 'OData-EntityId' not present.");
                }
            }
            else
            {
                ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
            }

            return entityId;
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebApiException"></exception>
        public virtual async Task UpdateAsync(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var navLink = entity.ToNavigationLink(WebApiMetadata);

            var json = JsonConvert.SerializeObject(entity, SerializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, navLink)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(entity.RowVersion))
            {
                httpRequest.Headers.IfMatch.Add(new EntityTagHeaderValue(entity.RowVersion));
            }

            using var httpResponse = await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, default)
                .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="target">Target entity reference</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebApiException"></exception>
        public virtual async Task DeleteAsync(EntityReference target)
        {
            _logger.LogDebug($"Start {nameof(DeleteAsync)}");
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var navLink = target.ToNavigationLink(WebApiMetadata);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, navLink);

            if (!string.IsNullOrEmpty(target.RowVersion))
            {
                httpRequest.Headers.IfMatch.Add(new EntityTagHeaderValue(target.RowVersion));
            }

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken: default)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
            
            ValidateResponseContent(httpResponse);
        }

        public virtual Task<TResponse> ExecuteAsync<TResponse>(IWebApiFunction apiFunctionRequest,
            CancellationToken cancellationToken = default)
        {
            if (apiFunctionRequest is null)
            {
                throw new ArgumentNullException(nameof(apiFunctionRequest));
            }

            var query = apiFunctionRequest.QueryString();

            return ExecuteFunctionAsync<TResponse>(query, cancellationToken);
        }

        public virtual Task<TResponse> ExecuteAsync<TResponse>(WebApiActionBase apiActionRequest,
            CancellationToken cancellationToken = default)
        {
            if (apiActionRequest == null)
                throw new ArgumentNullException(nameof(apiActionRequest));
            
            var query = apiActionRequest.Action;

            if (apiActionRequest.BoundEntity != null)
            {
                var entityMd = WebApiMetadata.GetEntityMetadata(apiActionRequest.BoundEntity.LogicalName);

                query = $"{apiActionRequest.BoundEntity.ToNavigationLink(WebApiMetadata)}/{apiActionRequest.Action}";
            }

            return ExecuteActionAsync<TResponse>(query, apiActionRequest.Parameters, cancellationToken);
        }
        
        public virtual Task ExecuteAsync(WebApiActionBase apiActionRequest, CancellationToken cancellationToken = default)
        {
            if (apiActionRequest == null)
                throw new ArgumentNullException(nameof(apiActionRequest));
            
            return ExecuteAsync<object>(apiActionRequest, cancellationToken);
        }

        public virtual async Task<TResponse> ExecuteActionAsync<TResponse>(string queryString, object parameters,
            CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(parameters, SerializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, queryString)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            
            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            var result = await ReadResponseAsync<TResponse>(httpResponse);

            return result;
        }

        public virtual async Task<TResponse> ExecuteFunctionAsync<TResponse>(string query,
            CancellationToken cancellationToken = default)
        {

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, query);

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            var result = await ReadResponseAsync<TResponse>(httpResponse);

            return result;
        }

        private void ValidateResponseContent(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                return;
            
            if ((httpResponse.RequestMessage?.RequestUri?.Segments?.LastOrDefault() ?? "")
                .ToUpperInvariant()
                .Equals("ERRORHANDLER.ASPX"))
            {
                var queryString = httpResponse.RequestMessage.RequestUri.Query;
                var qscoll = System.Web.HttpUtility.ParseQueryString(queryString);

                var sb = new StringBuilder("");
                foreach (var s in qscoll.AllKeys)
                {
                    sb.Append($"[{s}]: " + qscoll[s].Trim() + Environment.NewLine);
                }

                sb.Append("");
                
                var errorMessage = sb.ToString();
                
                _logger.LogError(errorMessage);
                throw new WebApiException(errorMessage);
            }

            var mediaType = httpResponse.Content.Headers.ContentType?.MediaType;
            
            if (!JsonMediaType.Equals(mediaType))
            {
                var errorMessage = $"The content type is not supported ({httpResponse.Content.Headers.ContentType}).";
                throw new WebApiException(errorMessage);
            }
        }

        private async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                return default;
            
            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);

            TResponse result = default;
            
            ValidateResponseContent(httpResponse);
            var contentStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var streamReader = new StreamReader(contentStream);
            using var jsonReader = new JsonTextReader(streamReader);
            try
            {
                result = _serializer.Deserialize<TResponse>(jsonReader);
            }
            catch (NotSupportedException ex) // When content type is not valid
            {
                _logger.LogError(ex, "The content type is not supported.");
            }
            catch (JsonException ex) // Invalid JSON
            {
                _logger.LogError(ex, "Invalid JSON.");
            }
            finally
            {
                streamReader.Close();
                jsonReader.Close();
            }
            
            return result;
        }
        
        public virtual Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Start RetrieveAsync at {entityName} with id = {id}");

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityName);

            var entityMd = WebApiMetadata.GetEntityMetadata(entityName);

            var request = $"{entityMd.EntitySetName}({id}){queryString}";


            var entity = ExecuteFunctionAsync<Entity>(request, cancellationToken);

            _logger.LogInformation($"Finish RetrieveAsync at {entityName} with id = {id}");

            return entity;
        }

        public virtual Task<Entity> RetrieveAsync(EntityReference entityReference,
            [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (entityReference is null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            var navLink = entityReference.ToNavigationLink(WebApiMetadata);

            _logger.LogInformation($"Start RetrieveAsync at {navLink}");

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityReference.LogicalName);

            var request = $"{navLink}{queryString}";

            var entity = ExecuteFunctionAsync<Entity>(request, cancellationToken);

            _logger.LogInformation($"Finish RetrieveAsync at {navLink}");

            return entity;
        }

        public virtual Task<EntityCollection> RetrieveMultipleAsync(string entityName,
            [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Start RetrieveMultipleAsync at {entityName}");

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityName);

            var entityMd = WebApiMetadata.GetEntityMetadata(entityName);

            var request = $"{entityMd.EntitySetName}{queryString}";

            return ExecuteFunctionAsync<EntityCollection>(request, cancellationToken);
        }

        public virtual Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml,
            CancellationToken cancellationToken = default)
        {
            if (fetchXml is null)
            {
                throw new ArgumentNullException(nameof(fetchXml));
            }

            var entityMetadata = WebApiMetadata.GetEntityMetadata(fetchXml.EntityName);

            var query = $"{entityMetadata.EntitySetName}?fetchXml={System.Net.WebUtility.UrlEncode(fetchXml)}";

            return ExecuteFunctionAsync<EntityCollection>(query, cancellationToken);
        }

        /// <summary>
        /// Clear Lookup field of entity
        /// </summary>
        /// <param name="target">Target entity reference</param>
        /// <param name="propertyName">Lookup property name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task DisassociateAsync(EntityReference target, string propertyName)
        {
            _logger.LogDebug($"Start {nameof(DisassociateAsync)}");
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var navLink = target.ToNavigationLink(WebApiMetadata);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{navLink}/{propertyName}/$ref");

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken: default)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest,
            HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (!Guid.Empty.Equals(CallerId))
            {
                httpRequest.Headers.TryAddWithoutValidation("MSCRMCallerID", CallerId.ToString());
            }

            if (httpRequest.Method != HttpMethod.Get)
            {
                return await HttpClient.SendAsync(httpRequest, completionOption, cancellationToken)
                    .ConfigureAwait(false);
            }

            httpRequest.Headers.TryAddWithoutValidation("Prefer", "odata.include-annotations=\"*\"");
            httpRequest.Headers.TryAddWithoutValidation("Prefer", $"odata.maxpagesize={MaxPageSize}");

            return await HttpClient.SendAsync(httpRequest, completionOption, cancellationToken).ConfigureAwait(false);
        }

        public Guid GetMyCrmUserId()
        {
            var response = ExecuteAsync<WhoAmIResponse>(new WhoAmIRequest(), cancellationToken: default)
                .GetAwaiter().GetResult();

            return response.UserId;
        }
    }
}