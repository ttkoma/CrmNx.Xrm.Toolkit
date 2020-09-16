using System;
using System.Diagnostics.CodeAnalysis;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.Serialization
{
    internal class EntityReferenceConverter : JsonConverter<EntityReference>
    {
        private WebApiMetadata _metadata;

        public EntityReferenceConverter(WebApiMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public override EntityReference ReadJson(JsonReader reader, Type objectType, [AllowNull] EntityReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] EntityReference value, JsonSerializer serializer)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "EntityReference can't be null.");
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (writer.WriteState == WriteState.Property)
            {
                writer.WriteRawValue($"\"{value.ToNavigationLink(_metadata)}\"");
            }
            else
            {
                throw new NotImplementedException("EntityReferenceConverter.WriteJson");
            }
        }
    }
}
