namespace CrmNx.Xrm.Toolkit.ObjectModel;

public class Relationship
{
    public Relationship()
    {
    }

    public Relationship(string schemaName)
    {
        SchemaName = schemaName;
    }
    
    public string SchemaName { get; set; } = string.Empty;
    
    public EntityRole? PrimaryEntityRole { get; set; }
}