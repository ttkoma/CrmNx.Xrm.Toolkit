using System;
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
                ? new EntityReference(entity.LogicalName, entity.KeyAttributes) {RowVersion = entity.RowVersion}
                : new EntityReference(entity.LogicalName, entity.Id) {RowVersion = entity.RowVersion};
        }

        public static string ToNavigationLink(this Entity entity, IWebApiMetadataService organizationMetadata)
        {
            return entity.ToEntityReference().ToNavigationLink(organizationMetadata);
        }

        public static TEntity ToEntity<TEntity>(this Entity otherEntity)
            where TEntity : Entity, new()
        {
            if (otherEntity == null)
                throw new ArgumentNullException(nameof(otherEntity));

            var entity = new TEntity
            {
                Id = otherEntity.Id,
                RowVersion = otherEntity.RowVersion
            };

            foreach (var (keyName, value) in otherEntity.KeyAttributes)
            {
                entity.KeyAttributes.Add(keyName, value);
            }

            foreach (var (keyName, value) in otherEntity.Attributes)
            {
                entity.Attributes.Add(keyName, value);
            }

            foreach (var (keyName, value) in otherEntity.FormattedValues)
            {
                entity.FormattedValues.Add(keyName, value);
            }

            return entity;
        }
    }
}