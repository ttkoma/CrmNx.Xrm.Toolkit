using CrmNx.Xrm.Toolkit.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CrmNx.Xrm.Toolkit.Serialization
{
    internal class EntityConverter : JsonConverter<Entity>
    {
        private readonly IWebApiMetadataService _metadata;

        public EntityConverter(IWebApiMetadataService metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public override Entity ReadJson(JsonReader reader, Type objectType, [AllowNull] Entity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                throw new NotImplementedException();
            }

            var jObject = JObject.Load(reader);
            var dictionary = jObject.ToObject<Dictionary<string, object>>();

            var entity = ODataResponseReader.ReadEntity(dictionary, _metadata, serializer);

            return entity;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Entity entity, JsonSerializer serializer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity can't be empty.");
            }

            var md = _metadata.GetEntityMetadata(entity.LogicalName);
            if (!entity.ContainsValue(md.PrimaryIdAttribute) && !Guid.Equals(entity.Id, Guid.Empty))
            {
                entity[md.PrimaryIdAttribute] = entity.Id;
            }

            writer.WriteStartObject();
            
            // ODataType
            writer.WritePropertyName("@odata.type");
            serializer.Serialize(writer, $"Microsoft.Dynamics.CRM.{entity.LogicalName}");

            foreach (var (attributeName, attributeValue) in entity.Attributes)
            {
                var propName = attributeName;
                var propValue = attributeValue;

                if (propValue is EntityReference)
                {
                    var relationMd = _metadata.GetRelationshipMetadata(rel =>
                        rel.ReferencingEntity.Equals(entity.LogicalName, StringComparison.OrdinalIgnoreCase)
                        && rel.ReferencingAttribute.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(relationMd?.ReferencingEntityNavigationPropertyName))
                    {
                        propName = $"{relationMd.ReferencingEntityNavigationPropertyName}@odata.bind";
                    }
                    else
                    {
                        propName = $"{attributeName}@odata.bind";
                    }
                }

                writer.WritePropertyName(propName);

                if (attributeValue is DateTime dateTime
                    && _metadata.IsDateOnlyAttribute(entity.LogicalName, propName))
                {
                    propValue = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                serializer.Serialize(writer, propValue);
            }
            writer.WriteEndObject();

            //serializer.Serialize(writer, entity.Attributes);
        }
    }
}
