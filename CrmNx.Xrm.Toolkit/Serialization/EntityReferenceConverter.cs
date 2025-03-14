﻿using CrmNx.Xrm.Toolkit.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CrmNx.Xrm.Toolkit.Serialization
{
    internal class EntityReferenceConverter : JsonConverter<EntityReference>
    {
        private readonly IWebApiMetadataService _metadata;

        public EntityReferenceConverter(IWebApiMetadataService metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public override EntityReference ReadJson(JsonReader reader, Type objectType, [AllowNull] EntityReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, EntityReference value, JsonSerializer serializer)
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
                var collectionName = _metadata.GetEntitySetName(value.LogicalName);
                var entityPath = value.GetPath(collectionName);
                
                writer.WriteRawValue($"\"{entityPath}\"");
            }
            else if (writer.WriteState == WriteState.Array)
            {
                writer.WriteRawValue(value.ToCrmBaseEntityString(_metadata));
            }
            else
            {
                throw new NotImplementedException($"EntityReferenceConverter.WriteJson: state = {writer.WriteState}");
            }
        }
    }
}
