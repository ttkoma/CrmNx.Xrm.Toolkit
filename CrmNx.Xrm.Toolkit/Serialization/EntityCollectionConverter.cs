using System;
using System.Diagnostics.CodeAnalysis;
using CrmNx.Xrm.Toolkit.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrmNx.Xrm.Toolkit.Serialization
{
    internal class EntityCollectionConverter : JsonConverter<EntityCollection>
    {

        private readonly IWebApiMetadataService _metadata;

        public EntityCollectionConverter(IWebApiMetadataService metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public override EntityCollection ReadJson(JsonReader reader, Type objectType, [AllowNull] EntityCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                throw new NotImplementedException();
            }

            var jObject = JObject.Load(reader);
            var collection = new EntityCollection();

            if (jObject.TryGetValue("@odata.count", out var count))
            {
                collection.Count = count.ToObject<int>();
            }

            if (jObject.TryGetValue("@odata.nextLink", out var nextLink))
            {
                collection.NextLink = nextLink.ToObject<Uri>();
            }

            if (jObject.TryGetValue("@Microsoft.Dynamics.CRM.fetchxmlpagingcookie", out var pagingcookie))
            {
                var xmlPagingCookie = System.Net.WebUtility.UrlDecode(pagingcookie.ToString());
                var xmlDoc = System.Xml.Linq.XDocument.Parse(xmlPagingCookie);
                if (xmlDoc.Document.Root.Attribute("pagingcookie") != null)
                {
                    collection.PagingCookie = System.Net.WebUtility.UrlDecode(xmlDoc.Document.Root.Attribute("pagingcookie").Value);
                }
                xmlDoc = null;
            }

            if (jObject.TryGetValue("@odata.context", out var context)
                && ODataResponseReader.TryParseCollectionName(context.ToString(), out var collectionName))
            {
                var entityMd = _metadata.GetEntityMetadata(x => x.EntitySetName == collectionName);
                collection.EntityName = entityMd.LogicalName;
            }

            var valueArr = jObject["value"];
            if (valueArr == null) return collection;

            foreach (var item in valueArr)
            {
                if (item is JObject jObjectEntity)
                {
                    if (context != null)
                    {
                        jObjectEntity.Add("@odata.context", context);
                    }

                    var entity = jObjectEntity.ToObject<Entity>(serializer);
                    collection.Entities.Add(entity);
                }
            }

            return collection;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] EntityCollection value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
