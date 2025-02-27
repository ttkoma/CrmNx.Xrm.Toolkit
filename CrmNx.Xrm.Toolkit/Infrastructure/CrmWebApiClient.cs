using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using CrmNx.Xrm.Toolkit.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class CrmWebApiClient : ICrmWebApiClient
    {
        protected readonly HttpClient HttpClient;
        private readonly IOptions<CrmClientSettings> _options;
        private readonly ILogger<CrmWebApiClient> _logger;
        private readonly JsonSerializer _serializer;

        public IWebApiMetadataService WebApiMetadata { get; }

        private const int MaxPageSize = 250;

        private static readonly string JsonMediaType = MediaTypeHeaderValue.Parse("application/json").MediaType;

        private JsonSerializerSettings _serializerSettingsDefault;
        public JsonSerializerSettings SerializerSettings =>
            _serializerSettingsDefault ??= new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

        public CrmWebApiClient(HttpClient httpClient, IWebApiMetadataService webApiMetadata, IOptions<CrmClientSettings> options,
            ILogger<CrmWebApiClient> logger)
        {
            _options = options;
            
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            httpClient.BaseAddress = _options.Value.BaseAddress;
            
            WebApiMetadata = webApiMetadata ?? throw new ArgumentNullException(nameof(webApiMetadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            
            SerializerSettings.Converters.Add(new EntityConverter(webApiMetadata));
            SerializerSettings.Converters.Add(new EntityCollectionConverter(webApiMetadata));
            SerializerSettings.Converters.Add(new EntityReferenceConverter(webApiMetadata));
            
            // SerializerSettings.Context = new StreamingContext(StreamingContextStates.Other, this);

            _serializer = JsonSerializer.Create(SerializerSettings);
        }

        /// <summary>
        /// Gets or sets the current caller Id.
        /// </summary>
        public Guid CallerId { get; set; }
        
        public Uri BaseAddress => _options.Value.BaseAddress;

        public virtual async Task<Guid> CreateAsync(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var watch = Stopwatch.StartNew();
            _logger.LogDebug("Starting {WebApiOperationName} {TargetEntity}", "CREATE", entity.LogicalName);

            var collectionName = WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var json = JsonConvert.SerializeObject(entity, SerializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{collectionName}")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, default)
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
                    _logger.LogError("Response header 'OData-EntityId' not present.");
                    throw new WebApiException("Response header 'OData-EntityId' not present.");
                }
            }
            else
            {
                ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
            }

            watch.Stop();
            _logger.LogInformation("Complete {WebApiOperationName} {TargetEntity} in {Elapsed:0.0}ms - {EntityId}",
                "CREATE", entity.LogicalName, watch.Elapsed.TotalMilliseconds, entityId);

            return entityId;
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(Entity entity, bool allowUpsert = false)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var watch = Stopwatch.StartNew();
            _logger.LogDebug("Starting {WebApiOperationName} {TargetEntity}", "UPDATE", entity.LogicalName);

            var navLink = entity.ToNavigationLink(WebApiMetadata);

            var json = JsonConvert.SerializeObject(entity, SerializerSettings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, navLink)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(entity.RowVersion))
            {
                var tagValue = entity.RowVersion.Replace("W/", "");
                if (!tagValue.StartsWith("\""))
                    tagValue = $"\"{tagValue}";

                if (!tagValue.EndsWith("\""))
                    tagValue = $"{tagValue}\"";

                httpRequest.Headers.IfMatch.Add(new EntityTagHeaderValue(tagValue, isWeak: true));
            }
            else if (!allowUpsert) // DISABLE UPSERT.
            {
                httpRequest.Headers.Add("If-Match", "*");
            }

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, default)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);

            watch.Stop();
            _logger.LogInformation("Complete {WebApiOperationName} {TargetEntity} in {Elapsed:0.0}ms", "UPDATE",
                entity.LogicalName, watch.Elapsed.TotalMilliseconds);
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
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var watch = Stopwatch.StartNew();
            _logger.LogDebug("Starting {WebApiOperationName} {TargetEntity}", "DELETE", target.LogicalName);


            var requestId = Guid.NewGuid();

            var navLink = target.ToNavigationLink(WebApiMetadata);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, navLink);

            if (!string.IsNullOrEmpty(target.RowVersion))
            {
                var tagValue = target.RowVersion.Replace("W/", "");
                httpRequest.Headers.IfMatch.Add(new EntityTagHeaderValue(tagValue, isWeak: true));
            }

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);

            ValidateResponseContent(httpResponse);

            watch.Stop();
            _logger.LogInformation("Complete {WebApiOperationName} {TargetEntity} in {Elapsed:0.0}ms", "DELETE",
                target.LogicalName, watch.Elapsed.TotalMilliseconds);
        }

        /// <inheritdoc/>
        public virtual async Task<TResponse> ExecuteAsync<TResponse>(OrganizationRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            var requestId = request.RequestId ?? Guid.NewGuid();

            using var httpRequest = new HttpRequestMessage();

            if (request.IsWebApiAction)
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = new Uri(request.RequestPath(), UriKind.Relative);

                var adjustParameters = request.Parameters
                    .Select(x => x.Value is EntityReference reference
                        ? KeyValuePair.Create(x.Key, (object)reference.ToCrmBaseEntity())
                        : x)
                    .ToDictionary(x => x.Key, x => x.Value);

                var json = JsonConvert.SerializeObject(adjustParameters, SerializerSettings);
                
                httpRequest.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }
            else
            {
                httpRequest.Method = HttpMethod.Get;
                var requestQuery = request.RequestPath();

                foreach (var parameter in request.Parameters)
                {
                    if (parameter.Value == null)
                        continue;

                    var serializedValue = parameter.Value;
                    if (parameter.Value is EntityReference entityRef)
                    {
                        serializedValue = new Dictionary<string, object>
                        {
                            { "@odata.id", entityRef }
                        };
                    }

                    var stringValue = JsonConvert.SerializeObject(serializedValue, SerializerSettings);

                    // FIXME: this is quick workaround for serialization Parameters to query string
                    if (stringValue.StartsWith("\""))
                    {
                        stringValue = stringValue.Substring(1, stringValue.Length - 2);
                    }


                    var queryParameterName = parameter.Key;
                    // IF This WebApi Function and Function is present 
                    // Append `@` before parameter name 
                    if (!string.IsNullOrEmpty(request.RequestName))
                    {
                        queryParameterName = $"@{queryParameterName}";
                    }

                    requestQuery = QueryHelpers.AddQueryString(requestQuery, $"{queryParameterName}", stringValue);
                }

                httpRequest.RequestUri = new Uri(requestQuery, UriKind.Relative);
            }

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            var result = await ReadResponseAsync<TResponse>(httpResponse, requestId);

            return result;
        }

        /// <inheritdoc/>
        public virtual Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();

            var queryString = (options ?? new QueryOptions()).BuildQueryString(WebApiMetadata, entityName);

            var entityMd = WebApiMetadata.GetEntityMetadata(entityName);

            var request = $"{entityMd.EntitySetName}({id}){queryString}";

            return GetAsync<Entity>(request, requestId, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<Entity> RetrieveAsync(EntityReference entityReference,
            [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (entityReference is null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            var requestId = Guid.NewGuid();

            var navLink = entityReference.ToNavigationLink(WebApiMetadata);

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityReference.LogicalName);

            var request = $"{navLink}{queryString}";

            return GetAsync<Entity>(request, requestId, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<EntityCollection> RetrieveMultipleAsync(string entityName,
            [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting {WebApiOperationName} at {TargetEntity}",
                "RetrieveMultiple", entityName);
            
            var requestId = Guid.NewGuid();

            var queryString = (options ?? new QueryOptions())
                .BuildQueryString(WebApiMetadata, entityName);

            var entityMd = WebApiMetadata.GetEntityMetadata(entityName);

            var request = $"{entityMd.EntitySetName}{queryString}";

            return GetAsync<EntityCollection>(request, requestId, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml,
            CancellationToken cancellationToken = default)
        {
            if (fetchXml is null)
            {
                throw new ArgumentNullException(nameof(fetchXml));
            }

            var requestId = Guid.NewGuid();

            var entityMetadata = WebApiMetadata.GetEntityMetadata(fetchXml.EntityName);
            var query = $"{entityMetadata.EntitySetName}?fetchXml={System.Net.WebUtility.UrlEncode(fetchXml)}";
            
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, query);
            
            if (fetchXml.IncludeAnnotations)
            {
                httpRequest.Headers.TryAddWithoutValidation("Prefer", "odata.include-annotations=\"*\"");
            }

            HttpMessageContent messageContent = ToMessageContent(httpRequest);
            var responseCollection = await SendBatchRequestAsync(new[] { messageContent }, cancellationToken).ConfigureAwait(false);

            httpRequest.Dispose();
            var result = await ReadResponseAsync<EntityCollection>(responseCollection.First(), requestId);

            return result;
        }
        
        /// <inheritdoc/>
        public async Task DisassociateAsync(EntityReference target, string propertyName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.LogicalName))
            {
                throw new ArgumentException("Entity Logical name cannot be empty.");
            }

            var requestId = Guid.NewGuid();

            var navLink = target.ToNavigationLink(WebApiMetadata);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{navLink}/{propertyName}/$ref");

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead,
                        CancellationToken.None)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            ODataResponseReader.EnsureSuccessStatusCode(httpResponse, _logger);
        }

        /// <inheritdoc/>
        public Guid GetMyCrmUserId()
        {
            var response = ExecuteAsync(new WhoAmIRequest(), cancellationToken: default)
                .GetAwaiter().GetResult();

            return response.UserId;
        }

        private async Task<TResponse> GetAsync<TResponse>(string query, Guid requestId,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, query);

            using var httpResponse =
                await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            httpRequest.Dispose();

            var result = await ReadResponseAsync<TResponse>(httpResponse, requestId);

            return result;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest,
            HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (!Guid.Empty.Equals(CallerId))
            {
                httpRequest.Headers.TryAddWithoutValidation("MSCRMCallerID", CallerId.ToString());
            }

            // TODO: Include only if needed
            if (httpRequest.Method == HttpMethod.Post // Execute Actions
                || httpRequest.Method == HttpMethod.Get)
            {
                httpRequest.Headers.TryAddWithoutValidation("Prefer", "odata.include-annotations=\"*\"");
            }
            
            if (httpRequest.Method == HttpMethod.Get)
            {
                httpRequest.Headers.TryAddWithoutValidation("Prefer", $"odata.maxpagesize={MaxPageSize}");
            }

            return await HttpClient.SendAsync(httpRequest, completionOption, cancellationToken).ConfigureAwait(false);
        }
        
        private void ValidateResponseContent(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                return;

            if ((httpResponse.RequestMessage?.RequestUri?.Segments?.LastOrDefault() ?? "")
                .ToUpperInvariant()
                .Equals("ERRORHANDLER.ASPX"))
            {
                var queryString = httpResponse.RequestMessage?.RequestUri?.Query ?? "";
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

        private async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage httpResponse, Guid requestId)
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
        
        private HttpMessageContent ToMessageContent(HttpRequestMessage httpRequests)
        {
            httpRequests.RequestUri = new Uri(baseUri: HttpClient.BaseAddress, relativeUri: httpRequests.RequestUri);
            
            if (httpRequests.Content != null)
            {
                if (httpRequests.Content.Headers.Contains("Content-Type"))
                {
                    httpRequests.Content.Headers.Remove("Content-Type");
                }
                httpRequests.Content.Headers.Add("Content-Type", "application/json;type=entry");
            }

            HttpMessageContent messageContent = new(httpRequests);
            
            if (messageContent.Headers.Contains("Content-Type"))
            {
                messageContent.Headers.Remove("Content-Type");
            }
            messageContent.Headers.Add("Content-Type", "application/http");
            messageContent.Headers.Add("Content-Transfer-Encoding", "binary");

            return messageContent;
        }
        
        private async Task<List<HttpResponseMessage>> ParseMultipartContent(HttpContent content, CancellationToken cancellationToken=default)
        {
            MultipartMemoryStreamProvider batchResponseContent = await content.ReadAsMultipartAsync(cancellationToken);
            List<HttpResponseMessage> responses = new();

            batchResponseContent?.Contents?.ToList().ForEach(async httpContent =>
            {
                // This is true for changesets
                if (httpContent.IsMimeMultipartContent())
                {
                    // Recursive call
                    responses.AddRange(await ParseMultipartContent(httpContent, cancellationToken));
                }
                // This is for individual responses outside of change set.
                else
                {
                    httpContent.Headers.Remove("Content-Type");
                    httpContent.Headers.Add("Content-Type", "application/http;msgtype=response");

                    HttpResponseMessage httpResponseMessage =
                        await httpContent.ReadAsHttpResponseMessageAsync(cancellationToken);

                    if (httpResponseMessage != null)
                    {
                        responses.Add(httpResponseMessage);
                    }
                }
            });

            return responses;
        }

        private async Task<List<HttpResponseMessage>> SendBatchRequestAsync(HttpMessageContent[] httpMessageContents, CancellationToken cancellationToken=default)
        {
            var batchRequest = new HttpRequestMessage(HttpMethod.Post, "$batch");
            var mixedContent = new MultipartContent("mixed", $"batch_{Guid.NewGuid():D}");

            foreach (var messageContent in httpMessageContents)
            {
                mixedContent.Add(messageContent);
            }

            batchRequest.Content = mixedContent;

            using var httpResponse =
                await SendAsync(batchRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            
            mixedContent.Dispose();
            batchRequest.Dispose();

            var responseCollection = await ParseMultipartContent(httpResponse.Content, cancellationToken)
                .ConfigureAwait(false);

            return responseCollection;
        }
    }
}