namespace CrmNx.Xrm.Toolkit.Metadata;

[MetadataName(LogicalCollectionName = "ManyToManyRelationshipDefinitions", LogicalName = nameof(ManyToManyRelationshipMetadata))]
public class ManyToManyRelationshipMetadata : RelationshipMetadataBase
{
    public ManyToManyRelationshipMetadata()
    {
        RelationshipType = RelationshipType.ManyToOneRelationship;
    }
    
    public string Entity1IntersectAttribute { get; set; } = string.Empty;
    public string Entity1LogicalName { get; set; } = string.Empty;
    public string Entity1NavigationPropertyName { get; set; } = string.Empty;
    
    public string Entity2IntersectAttribute { get; set; } = string.Empty;
    public string Entity2LogicalName { get; set; } = string.Empty;
    public string Entity2NavigationPropertyName { get; set; } = string.Empty;
    
    public string IntersectEntityName { get; set; } = string.Empty;
}