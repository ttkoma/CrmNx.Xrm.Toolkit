using System;

namespace CrmNx.Xrm.Toolkit.Metadata
{
    [MetadataName(LogicalCollectionName = "EntityDefinitions", LogicalName = "EntityMetadata")]
    public class EntityMetadata : MetadataBase
    {
        public string LogicalName { get; set; }

        public string EntitySetName { get; set; }

        public string PrimaryIdAttribute { get; set; }

        public string PrimaryNameAttribute { get; set; }

        public string SchemaName { get; set; }

        public int ObjectTypeCode { get; set; }
    }
}
