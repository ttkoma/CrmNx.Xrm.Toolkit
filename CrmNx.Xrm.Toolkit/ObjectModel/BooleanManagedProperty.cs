namespace CrmNx.Xrm.Toolkit.ObjectModel;

public class BooleanManagedProperty
{
    public bool CanBeChanged { get; set; }
    public string ManagedPropertyLogicalName { get; set; } = string.Empty;
    public bool Value { get; set; }
}