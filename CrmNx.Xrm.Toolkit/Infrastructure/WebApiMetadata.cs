using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Metadata;
using CrmNx.Xrm.Toolkit.ObjectModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class WebApiMetadata : IWebApiMetadataService
    {
        private ICollection<EntityMetadata> _entityMetadataDefinitions;
        private ICollection<OneToManyRelationshipMetadata> _oneToManyRelationships;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebApiMetadata> _logger;

        private readonly ConcurrentDictionary<string, DateTimeBehavior> _dateAttributesBehavior =
            new ConcurrentDictionary<string, DateTimeBehavior>();

        public WebApiMetadata(IServiceProvider serviceProvider, ILogger<WebApiMetadata> logger) : this(logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// For Support Testing Only
        /// </summary>
        protected WebApiMetadata(ILogger<WebApiMetadata> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public EntityMetadata GetEntityMetadata(Func<EntityMetadata, bool> predicate)
        {
            if (_entityMetadataDefinitions != null && _entityMetadataDefinitions.Any())
            {
                return _entityMetadataDefinitions.Where(predicate).FirstOrDefault();
            }

            var definitions = RetrieveEntityDefinitionsAsync().GetAwaiter().GetResult();
            SetEntityDefinitions(definitions);

            return _entityMetadataDefinitions.Where(predicate).FirstOrDefault();
        }

        public OneToManyRelationshipMetadata GetRelationshipMetadata(
            Func<OneToManyRelationshipMetadata, bool> predicate)
        {
            if (_oneToManyRelationships == null || !_oneToManyRelationships.Any())
            {
                var definitions = RetrieveOneToManyRelationshipsAsync().GetAwaiter().GetResult();
                SetOneToManyRelationshipsDefinitions(definitions);
            }

            var definition = _oneToManyRelationships.Where(predicate).FirstOrDefault();

            return definition;
        }

        public bool IsDateOnlyAttribute(string entityLogicalName, string attributeLogicalName)
        {
            var dateAttributeKey = $"{entityLogicalName}.{attributeLogicalName}"
                .ToLowerInvariant();

            if (_dateAttributesBehavior.ContainsKey(dateAttributeKey))
            {
                return _dateAttributesBehavior[dateAttributeKey] == DateTimeBehavior.DateOnly;
            }

            var dateTimeAttributeBehavior = GetDateTimeBehaviorAttributeAsync(entityLogicalName, attributeLogicalName)
                .GetAwaiter().GetResult();

            _dateAttributesBehavior.TryAdd(dateAttributeKey, dateTimeAttributeBehavior);

            return dateTimeAttributeBehavior == DateTimeBehavior.DateOnly;
        }

        protected virtual async Task<IEnumerable<EntityMetadata>> RetrieveEntityDefinitionsAsync()
        {
            _logger.LogDebug("Starting {WebApiOperationName}","RetrieveEntityDefinitions");
            var watch = Stopwatch.StartNew();

            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            // var queryString = $"{EntityMetadataPath}?$select={EntityMetadataFields}";
            var request = new OrganizationRequest<DataCollection<EntityMetadata>>
            {
                RequestBindingPath = "EntityDefinitions",
                Parameters =
                {
                    {
                        "$select",
                        string.Join(",", new[]
                        {
                            "SchemaName", "LogicalName", "EntitySetName", "PrimaryIdAttribute",
                            "PrimaryNameAttribute", "ObjectTypeCode"
                        })
                    }
                }
            };

            var collection = await GetCrmClient()
                .ExecuteAsync(request, cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);

            watch.Stop();
            _logger.LogInformation("Complete {WebApiOperationName} in {Elapsed:0.0} ms",
                "RetrieveEntityDefinitions", watch.Elapsed.TotalMilliseconds);

            return collection?.Items;
        }

        protected virtual async Task<IEnumerable<OneToManyRelationshipMetadata>> RetrieveOneToManyRelationshipsAsync()
        {
            _logger.LogDebug("Starting {WebApiOperationName}", "RetrieveOneToManyRelationships");
            var watch = Stopwatch.StartNew();
            
            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            // var query = $"{OneToManyRelationShipPath}?$select={OneToManyRelationshipFields}";
            var request =
                new OrganizationRequest<DataCollection<OneToManyRelationshipMetadata>>()
                {
                    RequestBindingPath = "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata",
                    Parameters =
                    {
                        {
                            "$select", string.Join(",", new[]
                            {
                                "ReferencingEntity", "ReferencingAttribute", "ReferencingEntityNavigationPropertyName",
                                "ReferencedEntity", "ReferencedAttribute", "ReferencedEntityNavigationPropertyName",
                                "SchemaName"
                            })
                        }
                    }
                };

            var collection = await GetCrmClient()
                .ExecuteAsync(request, CancellationToken.None)
                .ConfigureAwait(false);
            
            _logger.LogInformation("Complete {WebApiOperationName} in {Elapsed:0.0} ms",
                "RetrieveOneToManyRelationships", watch.Elapsed.TotalMilliseconds);

            return collection?.Items;
        }

        protected virtual async Task<DateTimeBehavior> GetDateTimeBehaviorAttributeAsync(string entityLogicalName,
            string attributeLogicalName)
        {
            _logger.LogDebug("Starting {WebApiOperationName} {Entity}.{Attribute}",
                "GetDateTimeBehavior",
                entityLogicalName,
                attributeLogicalName);

            var watch = Stopwatch.StartNew();

            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            //var query = string.Format(CultureInfo.InvariantCulture, CheckAttributeDateOnlyRequest, entityLogicalName,
            //    attributeLogicalName);

            var request = new OrganizationRequest<JObject>()
            {
                RequestBindingPath =
                    $"EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes(LogicalName='{attributeLogicalName}')/Microsoft.Dynamics.CRM.DateTimeAttributeMetadata",

                Parameters =
                {
                    // {"$select", "LogicalName,Format,DateTimeBehavior"},
                    { "$select", "LogicalName,Format,DateTimeBehavior" },
                    // {
                    //     "$filter",
                    //     "DateTimeBehavior ne null and Format eq Microsoft.Dynamics.CRM.DateTimeFormat'DateOnly'"
                    // }
                }
            };

            DateTimeBehavior behavior = default;

            try
            {
                var result = await GetCrmClient()
                    .ExecuteAsync(request, CancellationToken.None)
                    .ConfigureAwait(false);

                var stringValue = result["DateTimeBehavior"]?["Value"]?.ToString();
                if (Enum.TryParse(typeof(DateTimeBehavior), stringValue, true, out var parsedValue))
                {
                    behavior = (DateTimeBehavior)parsedValue;
                }
            }
            catch (Exception)
            {
                throw;
            }
            // catch (WebApiException ex)
            // {
            //     // if (Equals(ex.StatusCode, HttpStatusCode.NotFound))
            //     // {
            //     //     isDateOnly = false;
            //     // }
            //     // else
            //     // {
            //     //     throw;
            //     // }
            // }
            finally
            {
                watch.Stop();
                _logger.LogInformation(
                    "Complete {WebApiOperationName} {EntityName}.{AttributeName} - {DateTimeBehavior} in {Elapsed:0.0} ms",
                    "GetDateTimeBehavior",
                    entityLogicalName, attributeLogicalName, behavior, watch.Elapsed.TotalMilliseconds);
            }

            return behavior;
        }

        private void SetEntityDefinitions(IEnumerable<EntityMetadata> entityMetadataCollection)
        {
            _entityMetadataDefinitions = entityMetadataCollection?.ToArray() ??
                                         throw new ArgumentNullException(nameof(entityMetadataCollection));
        }

        private void SetOneToManyRelationshipsDefinitions(
            IEnumerable<OneToManyRelationshipMetadata> relationshipMetadataCollection)
        {
            _oneToManyRelationships = relationshipMetadataCollection?.ToArray() ??
                                      throw new ArgumentNullException(nameof(relationshipMetadataCollection));
        }

        private ICrmWebApiClient GetCrmClient()
        {
            return _serviceProvider.GetService(typeof(ICrmWebApiClient)) as ICrmWebApiClient;
        }
    }
}