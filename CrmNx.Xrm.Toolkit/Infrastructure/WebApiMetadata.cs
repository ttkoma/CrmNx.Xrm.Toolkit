using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Metadata;
using CrmNx.Xrm.Toolkit.ObjectModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class WebApiMetadata : IWebApiMetadataService
    {
        private ICollection<EntityMetadata> _entityMetadataDefinitions;
        private ICollection<OneToManyRelationshipMetadata> _oneToManyRelationships;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebApiMetadata> _logger;

        private readonly IList<KeyValuePair<string, string>> _dateOnlyAttributes =
            new List<KeyValuePair<string, string>>();

        private const string EntityMetadataPath = "EntityDefinitions";

        private static readonly string EntityMetadataFields = string.Join(",", new[]
        {
            "SchemaName", "LogicalName", "EntitySetName", "PrimaryIdAttribute",
            "PrimaryNameAttribute", "ObjectTypeCode"
        });

        private const string OneToManyRelationShipPath =
            "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata";

        private static readonly string OneToManyRelationshipFields = string.Join(",", new[]
        {
            "ReferencingEntity", "ReferencingAttribute", "ReferencingEntityNavigationPropertyName",
            "ReferencedEntity", "ReferencedAttribute", "ReferencedEntityNavigationPropertyName",
            "SchemaName"
        });

        private const string CheckAttributeDateOnlyRequest =
            "EntityDefinitions(LogicalName='{0}')/Attributes(LogicalName='{1}')/Microsoft.Dynamics.CRM.DateTimeAttributeMetadata?$filter=DateTimeBehavior ne null and Format eq Microsoft.Dynamics.CRM.DateTimeFormat'DateOnly'&$select=LogicalName,Format,DateTimeBehavior";

        public WebApiMetadata(IServiceProvider serviceProvider, ILogger<WebApiMetadata> logger) : this(logger)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
                return _entityMetadataDefinitions.Where(predicate).FirstOrDefault();

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

        public virtual bool IsDateOnlyAttribute(string entityLogicalName, string attributeLogicalName)
        {
            if (_dateOnlyAttributes.Contains(
                new KeyValuePair<string, string>(entityLogicalName, attributeLogicalName)))
            {
                return true;
            }

            var isDateOnlyCheckResult = SearchDateOnlyAttributeAsync(entityLogicalName, attributeLogicalName)
                .GetAwaiter().GetResult();

            if (isDateOnlyCheckResult)
                _dateOnlyAttributes.Add(new KeyValuePair<string, string>(entityLogicalName, attributeLogicalName));

            return isDateOnlyCheckResult;
        }

        protected virtual async Task<IEnumerable<EntityMetadata>> RetrieveEntityDefinitionsAsync()
        {
            _logger.LogDebug("Start RetrieveEntityDefinitionsAsync");

            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            var queryString = $"{EntityMetadataPath}?$select={EntityMetadataFields}";

            var collection = await GetCrmClient()
                .ExecuteFunctionAsync<DataCollection<EntityMetadata>>(queryString, cancellationToken: default)
                .ConfigureAwait(false);

            _logger.LogDebug("Finish RetrieveEntityDefinitionsAsync");

            return collection?.Items;
        }

        protected virtual async Task<IEnumerable<OneToManyRelationshipMetadata>> RetrieveOneToManyRelationshipsAsync()
        {
            _logger.LogInformation("Start RetrieveOneToManyRelationshipsAsync");

            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            var query = $"{OneToManyRelationShipPath}?$select={OneToManyRelationshipFields}";

            var collection = await GetCrmClient()
                .ExecuteFunctionAsync<DataCollection<OneToManyRelationshipMetadata>>(query)
                .ConfigureAwait(false);

            _logger.LogInformation("Finish RetrieveOneToManyRelationshipsAsync");

            return collection?.Items;
        }

        private async Task<bool> SearchDateOnlyAttributeAsync(string entityLogicalName, string attributeLogicalName)
        {
            _logger.LogInformation("Start CheckAttributeIsDateOnly");

            // TODO: FIXME - Direct build query with out extensions used WebApiMetadata for disable IoC loop!!!
            var query = string.Format(CultureInfo.InvariantCulture, CheckAttributeDateOnlyRequest, entityLogicalName,
                attributeLogicalName);

            bool isDateOnly;

            try
            {
                var result = await GetCrmClient()
                    .ExecuteFunctionAsync<JObject>(query)
                    .ConfigureAwait(false);

                isDateOnly = result["DateTimeBehavior"]?["Value"]?.ToString() == "DateOnly";
            }
            catch (WebApiException ex)
            {
                if (Equals(ex.StatusCode, HttpStatusCode.NotFound))
                    isDateOnly = false;
                else
                    throw;
            }

            _logger.LogInformation("Finish CheckAttributeIsDateOnly");

            return isDateOnly;
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