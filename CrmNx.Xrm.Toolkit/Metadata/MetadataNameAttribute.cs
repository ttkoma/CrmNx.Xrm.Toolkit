using System;

namespace CrmNx.Xrm.Toolkit.Metadata
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataNameAttribute : Attribute
    {
        public string LogicalCollectionName { get; set; }

        public string LogicalName { get; set; }
    }
}
