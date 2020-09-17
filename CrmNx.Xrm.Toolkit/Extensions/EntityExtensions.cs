﻿using System;
using System.Linq;
using CrmNx.Xrm.Toolkit.Infrastructure;

namespace CrmNx.Xrm.Toolkit
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Convert entity To EntityReference
        /// </summary>
        /// <param name="entity">Converted entity</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If entity is null</exception>
        public static EntityReference ToEntityReference(this Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.KeyAttributes.Any()
                ? new EntityReference(entity.LogicalName, entity.KeyAttributes)
                : new EntityReference(entity.LogicalName, entity.Id);
        }

        public static string ToNavigationLink(this Entity entity, IWebApiMetadataService organizationMetadata)
        {
            return entity.ToEntityReference().ToNavigationLink(organizationMetadata);
        }
    }
}