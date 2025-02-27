using CrmNx.Xrm.Toolkit.ObjectModel;

namespace CrmNx.Xrm.Toolkit.Metadata;

public class RelationshipMetadataBase : MetadataBase
{
    public string IntroducedVersion { get; set; } = string.Empty;
    public BooleanManagedProperty IsCustomizable { get; set; }
    public bool IsCustomRelationship { get; set; }
    public bool IsManaged { get; set; }
    public bool IsValidForAdvancedFind { get; set; }
    public RelationshipType RelationshipType { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public SecurityTypes SecurityTypes { get; set; }
}