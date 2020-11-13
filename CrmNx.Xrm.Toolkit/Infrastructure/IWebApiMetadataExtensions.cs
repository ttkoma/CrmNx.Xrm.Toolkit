using CrmNx.Xrm.Toolkit.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    internal static class WebApiMetadataExtensions
    {
        public static string FormatPropertyToLogicalName(this IWebApiMetadataService metadata, string entityLogicalName,
            string propertyLogicalName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var relationship = metadata.GetRelationshipMetadata(x =>
                string.Equals(x.ReferencingEntity, entityLogicalName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ReferencingAttribute, propertyLogicalName, StringComparison.OrdinalIgnoreCase));

            return relationship != null ? $"_{propertyLogicalName}_value" : propertyLogicalName;
        }

        public static EntityMetadata GetEntityMetadata(this IWebApiMetadataService metadata,
            [AllowNull] string logicalName)
        {
            if (logicalName is null)
            {
                return default;
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var definition = metadata.GetEntityMetadata
                (x => string.Equals(x.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase));

            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"WebApiMetadata doesnt contains EntityDefinitions for entity {logicalName}.");
            }

            return definition;
        }

        /// <summary>
        /// Return EntitySetName by LogicalName
        /// </summary>
        /// <param name="metadata">Organization Metadata</param>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When metadata is null</exception>
        public static string GetEntitySetName(this IWebApiMetadataService metadata, string logicalName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            return metadata.GetEntityMetadata(logicalName).EntitySetName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="entitySetName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetEntityLogicalName(this IWebApiMetadataService metadata, string entitySetName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            return metadata.GetEntityMetadata(
                    x => string.Equals(x.EntitySetName, entitySetName, StringComparison.OrdinalIgnoreCase))
                .LogicalName;
        }

        public static string GetNavigationPropertyName(this IWebApiMetadataService metadata, string entityName,
            string attributeLogicalName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var definition = metadata.GetRelationshipMetadata(
                x => string.Equals(x.ReferencingEntity, entityName, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(x.ReferencingAttribute, attributeLogicalName, StringComparison.OrdinalIgnoreCase)
            );

            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"WebApiMetadata doesnt contains OneToManyRelationshipDefinitions for attribute {entityName}.{attributeLogicalName}.");
            }

            return definition.ReferencingEntityNavigationPropertyName;
        }

        public static string GetRelationshipSchemaName(this IWebApiMetadataService metadata, string entityName,
            string referencingAttributeName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var definition = metadata.GetRelationshipMetadata(
                x => string.Equals(x.ReferencingEntity, entityName, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(x.ReferencingAttribute, referencingAttributeName,
                         StringComparison.OrdinalIgnoreCase));

            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"WebApiMetadata doesnt contains OneToManyRelationshipDefinitions for attribute {entityName}.{referencingAttributeName}.");
            }

            return definition.SchemaName;
        }
    }
}