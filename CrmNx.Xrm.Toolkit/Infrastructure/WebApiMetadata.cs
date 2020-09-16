using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Metadata;
using CrmNx.Xrm.Toolkit.ObjectModel;
using Microsoft.Extensions.Logging;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class WebApiMetadata
    {
        private ICollection<EntityMetadata> _entityMetadatas;
        private ICollection<OneToManyRelationshipMetadata> _oneToManyRelationships;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebApiMetadata> _logger;

        private const string EntityMetadataPath = "EntityDefinitions";

        private static readonly string EntityMetadataFields = string.Join(",", new [] {
            "SchemaName","LogicalName","EntitySetName","PrimaryIdAttribute",
            "PrimaryNameAttribute","ObjectTypeCode"
        });

        private const string OneToManyRelationShipPath = "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata";

        private static readonly string OneToManyRelationshipFields = string.Join(",", new [] {
                "ReferencingEntity","ReferencingAttribute","ReferencingEntityNavigationPropertyName",
                "ReferencedEntity","ReferencedAttribute","ReferencedEntityNavigationPropertyName",
                "SchemaName"
        });

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
            if (_entityMetadatas == null || !_entityMetadatas.Any())
            {
                var definitions = RetrieveEntityDefinitionsAsync().GetAwaiter().GetResult();
                SetEntityDefinitions(definitions);
            }

            return _entityMetadatas.Where(predicate).FirstOrDefault();
        }

        public OneToManyRelationshipMetadata GetRelationshipMetadata(Func<OneToManyRelationshipMetadata, bool> predicate)
        {
            if (_oneToManyRelationships == null || !_oneToManyRelationships.Any())
            {
                var definitions = RetrieveOneToManyRelationshipsAsync().GetAwaiter().GetResult();
                SetOneToManyRelationshipsDefinitions(definitions);
            }

            var definition = _oneToManyRelationships.Where(predicate).FirstOrDefault();

            return definition;
        }

        protected virtual async Task<IEnumerable<EntityMetadata>> RetrieveEntityDefinitionsAsync()
        {
            _logger.LogDebug("Start RetrieveEntityDefinitionsAsync");

            // Direct build query with out extensions used WebApiMetada for disable IoC loop!!!
            var queryString = $"{EntityMetadataPath}?$select={EntityMetadataFields}";

            var collection = await GetCrmClient()
                .ExecuteFunctionAsync<DataCollection<EntityMetadata>>(queryString, cancelationToken: default)
                .ConfigureAwait(false);

            _logger.LogDebug("Finish RetrieveEntityDefinitionsAsync");

            return collection?.Items;
        }

        protected virtual async Task<IEnumerable<OneToManyRelationshipMetadata>> RetrieveOneToManyRelationshipsAsync()
        {
            _logger.LogInformation("Start RetrieveOneToManyRelationshipsAsync");

            // Direct build query with out extensions used WebApiMetada for disable IoC loop!!!
            string query = $"{OneToManyRelationShipPath}?$select={OneToManyRelationshipFields}";

            var collection = await GetCrmClient()
                .ExecuteFunctionAsync<DataCollection<OneToManyRelationshipMetadata>>(query)
                .ConfigureAwait(false);

            _logger.LogInformation("Finish RetrieveOneToManyRelationshipsAsync");

            return collection?.Items;
        }

        private void SetEntityDefinitions(IEnumerable<EntityMetadata> entityMetadatas)
        {

            _entityMetadatas = entityMetadatas?.ToArray() ?? throw new ArgumentNullException(nameof(entityMetadatas));
        }

        private void SetOneToManyRelationshipsDefinitions(IEnumerable<OneToManyRelationshipMetadata> relationshipMetadatas)
        {
            _oneToManyRelationships = relationshipMetadatas?.ToArray() ?? throw new ArgumentNullException(nameof(relationshipMetadatas));
        }

        private ICrmClient GetCrmClient()
        {
            return _serviceProvider.GetService(typeof(ICrmClient)) as ICrmClient;
        }
    }
}
