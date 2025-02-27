using System;

namespace CrmNx.Xrm.Toolkit.Metadata;

public class MetadataBase
{
    public bool HasChanged { get; set; }
    public Guid MetadataId { get; set; }
}