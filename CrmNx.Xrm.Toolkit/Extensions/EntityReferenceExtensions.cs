﻿using CrmNx.Xrm.Toolkit.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmNx.Xrm.Toolkit
{
    internal static class EntityReferenceExtensions
    {
        /// <summary>
        /// Convert EntityReference to WebApi NavigationUrl
        /// </summary>
        /// <param name="entityReference">EntityReference</param>
        /// <param name="organizationMetadata">Instance metadata store</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When EntityReference is null</exception>
        [Obsolete("Use GetPath instead")]
        public static string ToNavigationLink(this EntityReference entityReference,
            IWebApiMetadataService organizationMetadata)
        {
            if (entityReference is null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            var logicalName = entityReference.LogicalName;
            var collectionName = organizationMetadata.GetEntitySetName(logicalName);

            // If alternate keys not present
            if (entityReference.KeyAttributes.Any() != true)
            {
                return $"{collectionName}({entityReference.Id})";
            }

            // Else If alternate keys present
            var keysPairList = new List<string>();

            foreach (var (key, value) in entityReference.KeyAttributes)
            {
                if (value is int)
                {
                    keysPairList.Add($"{key}={value}");
                }
                else
                {
                    keysPairList.Add($"{key}='{value}'");
                }
            }

            return $"{collectionName}({string.Join("&", keysPairList)})";
        }

        public static string GetPath(this EntityReference entityReference, string entitySetName)
        {
            if (entityReference.KeyAttributes.Any() && entityReference.Id == Guid.Empty)
            {
                var keysPairList = new List<string>();

                foreach (var (key, value) in entityReference.KeyAttributes)
                {
                    keysPairList.Add(value is int ? $"{key}={value}" : $"{key}='{value}'");
                }

                return $"{entitySetName}({string.Join(",", keysPairList)})";
            }
            
            return $"{entitySetName}({entityReference.Id})";
        }

        public static string AsODataId(this EntityReference entityReference, string entitySetName, Uri? baseAddress = null)
        {
            var path = entityReference.GetPath(entitySetName);
            if(baseAddress == null)
                return $"{{ \"@odata.id\": \"{path}\"}}";
            
            return $"{{ \"@odata.id\": \"{baseAddress}{path}\"}}";
        }

        public static string ToCrmBaseEntityString(this EntityReference entityReference,
            IWebApiMetadataService organizationMetadata)
        {
            if (entityReference is null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            var logicalName = entityReference.LogicalName;
            var idAttributeName = organizationMetadata.GetEntityMetadata(logicalName)?.PrimaryIdAttribute;
            var sb = new StringBuilder("{");
            sb.Append($"\"@odata.type\": \"Microsoft.Dynamics.CRM.{logicalName}\"");
            sb.Append(',');
            sb.Append($"\"{idAttributeName}\": \"{entityReference.Id}\"");
            sb.Append('}');
            
            return sb.ToString();
        }

        public static Entity ToCrmBaseEntity(this EntityReference entityReference)
        {
            if (entityReference is null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            var entity = new Entity(entityReference.LogicalName, entityReference.Id);

            foreach (var keyAttribute in entityReference.KeyAttributes)
            {
                entity.KeyAttributes.Add(keyAttribute.Key, keyAttribute.Value);
            }

            return entity;
        }
    }
}