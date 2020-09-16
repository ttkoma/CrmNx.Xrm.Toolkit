using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrmNx.Xrm.Toolkit.Serialization
{
    internal class EntityConverter : JsonConverter<Entity>
    {
        private WebApiMetadata _metadata;

        public EntityConverter(WebApiMetadata metadata)
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

            var entity = ODataResponseReader.ReadEntity(dictionary, ref _metadata, serializer);

            return entity;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Entity value, JsonSerializer serializer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Entity can't be empty.");
            }

            var md = _metadata.GetEntityMetadata(value.LogicalName);
            if (!value.ContainsValue(md.PrimaryIdAttribute) && !Guid.Equals(value.Id, Guid.Empty))
            {
                value[md.PrimaryIdAttribute] = value.Id;
            }

            writer.WriteStartObject();
            foreach (var property in value.Attributes)
            {
                var propertyName = property.Key;

                if (property.Value is EntityReference)
                {
                    propertyName = $"{property.Key}@odata.bind";
                }

                writer.WritePropertyName(propertyName);
                serializer.Serialize(writer, property.Value);
            }
            writer.WriteEndObject();

            //serializer.Serialize(writer, value.Attributes);
        }
    }
}
