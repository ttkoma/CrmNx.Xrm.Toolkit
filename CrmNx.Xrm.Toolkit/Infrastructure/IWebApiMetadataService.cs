using CrmNx.Xrm.Toolkit.Metadata;
using System;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public interface IWebApiMetadataService
    {
        EntityMetadata GetEntityMetadata(Func<EntityMetadata, bool> predicate);

        OneToManyRelationshipMetadata GetRelationshipMetadata(Func<OneToManyRelationshipMetadata, bool> predicate);

        bool IsDateOnlyAttribute(string entityLogicalName, string attributeLogicalName);
    }
}