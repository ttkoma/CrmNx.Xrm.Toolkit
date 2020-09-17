using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using CrmNx.Xrm.Toolkit.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class CrmWebApiClient : ICrmClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CrmWebApiClient> _logger;
        private readonly JsonSerializer _serializer;
        private readonly JsonSerializerSettings _serializerSettings;

        public IWebApiMetadataService WebApiMetadata { get; }

        private const int MaxPageSize = 250;

        public CrmWebApiClient(HttpClient httpClient, IWebApiMetadataService webApiMetadata,
            ILogger<CrmWebApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            WebApiMetadata = webApiMetadata ?? throw new ArgumentNullException(nameof(webApiMetadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.Converters.Add(new EntityConverter(webApiMetadata));
            _serializerSettings.Converters.Add(new EntityCollectionConverter(webApiMetadata));
            _serializerSettings.Converters.Add(new EntityReferenceConverter(webApiMetadata));
            _serializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
            _serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            _serializer = JsonSerializer.Create(_serializerSettings);
        }

        /// <summary>
        /// Gets or sets the current caller Id.
        /// </summary>
        public Guid CallerId { get; set; }

        public async Task<Guid> CreateAsync(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var collectionName = WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var json = JsonConvert.SerializeObject(entity, _serializerSettings);

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

        public async Task UpdateAsync(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var collectionName = WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var json = JsonConvert.SerializeObject(entity, _serializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"{collectionName}({entity.Id})")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            using var httpResponse = await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, default)
                .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
        }

        public async Task DeleteAsync(string entityName, Guid id)
        {
            var collectionName = WebApiMetadata.GetEntitySetName(entityName);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{collectionName}({id})");
            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken: default)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
        }

        public Task<TResponse> ExecuteAsync<TResponse>(IWebApiFunction apiFunctionRequest,
            CancellationToken cancellationToken = default)
        {
            if (apiFunctionRequest is null)
            {
                throw new ArgumentNullException(nameof(apiFunctionRequest));
            }

            var query = apiFunctionRequest.QueryString();

            return ExecuteFunctionAsync<TResponse>(query, cancellationToken);
        }

        public Task<TResponse> ExecuteAsync<TResponse>(IWebApiAction apiActionRequest,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Coming soon");
        }

        public Task<TResponse> ExecuteActionAsync<TResponse>(string query, object parameters,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Coming soon");
        }

        public async Task<TResponse> ExecuteFunctionAsync<TResponse>(string query,
            CancellationToken cancellationToken = default)
        {
            TResponse result = default;

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, query);

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);

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

        public Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null,
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

        public Task<Entity> RetrieveAsync(EntityReference entityReference, [AllowNull] QueryOptions options = null,
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

        public Task<EntityCollection> RetrieveMultipleAsync(string entityName, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Start RetrieveMultipleAsync at {entityName}");

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityName);

            var entityMd = WebApiMetadata.GetEntityMetadata(entityName);

            var request = $"{entityMd.EntitySetName}{queryString}";

            return ExecuteFunctionAsync<EntityCollection>(request, cancellationToken);
        }

        public Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml,
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


        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest,
            HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (!Guid.Empty.Equals(CallerId))
            {
                httpRequest.Headers.TryAddWithoutValidation("MSCRMCallerID", CallerId.ToString());
            }

            if (httpRequest.Method != HttpMethod.Get)
            {
                return await _httpClient.SendAsync(httpRequest, completionOption, cancellationToken)
                    .ConfigureAwait(false);
            }

            httpRequest.Headers.TryAddWithoutValidation("Prefer", "odata.include-annotations=\"*\"");
            httpRequest.Headers.TryAddWithoutValidation("Prefer", $"odata.maxpagesize={MaxPageSize}");

            return await _httpClient.SendAsync(httpRequest, completionOption, cancellationToken).ConfigureAwait(false);
        }

        public Guid GetMyCrmUserId()
        {
            var response = ExecuteAsync<WhoAmIResponse>(new WhoAmIRequest(), cancellationToken: default)
                .GetAwaiter().GetResult();

            return response.UserId;
        }
    }
}