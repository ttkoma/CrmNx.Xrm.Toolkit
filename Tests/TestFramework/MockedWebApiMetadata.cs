using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Metadata;
using CrmNx.Xrm.Toolkit.ObjectModel;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestFramework
{
    public sealed class MockedWebApiMetadata : WebApiMetadata
    {
        private readonly string _entitiesMetadataFileName;
        private readonly string _oneToManyRelationshipsFileName;

        /// <summary>
        /// Create Service Contains Metadata definitions for Dynamics Testing Environment
        /// </summary>
        /// <param name="entitiesMetadataFileName">
        /// File name at JsonContent directory with collection of EnityDefinitions testing Dynamics instance
        /// To obtain file for your organization call 
        /// GET https://host.local/orgName/api/data/v8.2/EntityDefinitions?$select=SchemaName,LogicalName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode 
        /// and save content to $(SolutionDirectory)\Tests\TestFramework\JsonContent\
        /// </param>
        /// <param name="oneToManyRelationshipsFileName">
        /// File name at JsonContent directory with collection of EnityDefinitions testing Dynamics instance
        /// To obtain file for your organization call 
        /// https://host/orgName/api/data/v8.2/RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata?$select=SchemaName,ReferencingEntity,ReferencingAttribute,ReferencingEntityNavigationPropertyName,ReferencedEntity,ReferencedAttribute,ReferencedEntityNavigationPropertyName
        /// and save content to $(SolutionDirectory)\Tests\TestFramework\JsonContent\
        /// </param>
        public MockedWebApiMetadata(string entitiesMetadataFileName, string oneToManyRelationshipsFileName) 
            : base(new NullLogger<MockedWebApiMetadata>())
        {
            _entitiesMetadataFileName = entitiesMetadataFileName;
            _oneToManyRelationshipsFileName = oneToManyRelationshipsFileName;
        }

        /// <summary>
        /// Create metadata store contains definitions from Dynamics 365 CE (on-premise 8.2)
        /// </summary>
        /// <returns></returns>
        public static MockedWebApiMetadata CreateD365CE()
        {
            return new MockedWebApiMetadata("EntityDefinitions.D365CE.json", "OneToManyRelationships.D365CE.json");
        } 

        protected override Task<IEnumerable<EntityMetadata>> RetrieveEntityDefinitionsAsync()
        {
            return Task.FromResult(LoadEntityMetadata);
        }

        protected override Task<IEnumerable<OneToManyRelationshipMetadata>> RetrieveOneToManyRelationshipsAsync()
        {
            return Task.FromResult(LoadOneToManyRelationships);
        }

        // Get EntityMetadatas for our organization
        // https://host/orgName/api/data/v8.2/EntityDefinitions?$select=SchemaName,LogicalName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode
        private IEnumerable<EntityMetadata> LoadEntityMetadata
        {
            get
            {
                var fileContent = Setup.GetSetupJsonContent(_entitiesMetadataFileName);
                var collection = Newtonsoft.Json.JsonConvert.DeserializeObject<DataCollection<EntityMetadata>>(fileContent);

                return collection.Items.ToArray();
            }
        }

        // Get OneToManyRelationShips Metadata for our organization
        // https://host/orgName/api/data/v8.2/RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata?$select=SchemaName,ReferencingEntity,ReferencingAttribute,ReferencingEntityNavigationPropertyName,ReferencedEntity,ReferencedAttribute,ReferencedEntityNavigationPropertyName
        private IEnumerable<OneToManyRelationshipMetadata> LoadOneToManyRelationships
        {
            get
            {
                var fileContent = Setup.GetSetupJsonContent(_oneToManyRelationshipsFileName);
                var collection = Newtonsoft.Json.JsonConvert.DeserializeObject<DataCollection<OneToManyRelationshipMetadata>>(fileContent);

                return collection.Items;
            }
        }
 
    }
}
