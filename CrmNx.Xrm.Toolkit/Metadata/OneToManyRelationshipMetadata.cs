using System;

namespace CrmNx.Xrm.Toolkit.Metadata
{
    [MetadataName(LogicalCollectionName = "OneToManyRelationshipDefinitions", LogicalName = "OneToManyRelationshipMetadata")]
    public class OneToManyRelationshipMetadata : RelationshipMetadataBase
    {
        public OneToManyRelationshipMetadata()
        {
            RelationshipType = RelationshipType.OneToManyRelationship;
        }
        
        public string ReferencingEntity { get; set; }
        public string ReferencingAttribute { get; set; }
        public string ReferencingEntityNavigationPropertyName { get; set; }
        
        public string ReferencedEntity { get; set; }
        public string ReferencedAttribute { get; set; }
        
        public string ReferencedEntityNavigationPropertyName { get; set; }
    }
}
