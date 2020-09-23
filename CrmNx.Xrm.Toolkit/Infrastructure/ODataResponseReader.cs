using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    internal static class ODataResponseReader
    {
        public static Entity ReadEntity(IDictionary<string, object> attributes, IWebApiMetadataService metadata,
            JsonSerializer jsonSerializer)
        {
            var toRemove = new List<string>();

            var entity = new Entity();

            // Получим Etag
            if (attributes.TryGetValue("@odata.etag", out var etagRaw))
            {
                entity.RowVersion = etagRaw.ToString();
                toRemove.Add("@odata.etag");
            }

            // Получим Контект
            if (attributes.TryGetValue("@odata.context", out var odataContext))
            {
                if (TryParseCollectionName(odataContext.ToString(), out var entitySetName))
                {
                    //entity.EntitySetName = entitySetName;
                    var md = metadata
                        .GetEntityMetadata(x =>
                            string.Equals(x.EntitySetName, entitySetName, StringComparison.OrdinalIgnoreCase));

                    if (md != null)
                    {
                        entity.LogicalName = md.LogicalName;
                        toRemove.Add("@odata.context");

                        if (attributes.ContainsKey(md.PrimaryIdAttribute))
                        {
                            entity.Id = new Guid(attributes[md.PrimaryIdAttribute].ToString());
                        }
                    }
                }
            }

            // Отберём все lookuplogicalname
            var entityReferenceAttributes = attributes
                .Where(a => a.Key.EndsWith("@Microsoft.Dynamics.CRM.lookuplogicalname",
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Обходим коллекцию Ссылочных аттрибутов
            foreach (var (attribute, value) in entityReferenceAttributes)
            {
                var key = attribute.Split('@')[0]; //_organizationid_value

                //_organizationid_value,
                //_organizationid_value@...lookuplogicalname,
                //_organizationid_value@...associatednavigationproperty
                //_organizationid_value@...FormattedValue
                var complements = attributes
                    .Where(a => a.Key.StartsWith(key, StringComparison.Ordinal)).ToArray();

                var logicalName = value.ToString();
                var id = Guid.Parse(complements.First(a => a.Key == key).Value.ToString());

                var entityReference = new EntityReference(logicalName, id)
                {
                    Name = complements.FirstOrDefault(c => IsFormatedValue(c.Key)).Value?.ToString(),

                    LogicalName = complements
                        .FirstOrDefault(c =>
                            c.Key.EndsWith("@Microsoft.Dynamics.CRM.lookuplogicalname", StringComparison.Ordinal))
                        .Value?.ToString()
                };

                var navPropertyName = FormatAttributeName(key);

                entity.Attributes.Add(navPropertyName, entityReference);
                entity.FormattedValues.Add(navPropertyName, entityReference.Name);

                toRemove.AddRange(complements.Select(x => x.Key).ToArray());
            }

            var formattedValueAttributeKeys = attributes.Keys
                .Where(x => x.EndsWith("@OData.Community.Display.V1.FormattedValue",
                    StringComparison.OrdinalIgnoreCase))
                .Except(toRemove)
                .ToArray();

            foreach (var key in formattedValueAttributeKeys)
            {
                entity.FormattedValues.Add(ParseFormatedValueProperty(key), attributes[key]?.ToString());
                toRemove.Add(key);
            }

            var anyKeys = attributes.Keys.Except(toRemove).ToArray();

            // Обходим оставшиеся аттрибуты
            foreach (var attributeKey in anyKeys)
            {
                var attributeValue = attributes[attributeKey];

                // Если это вложенный массив других сущностей
                if (attributeValue is IDictionary<string, object>[] nestedEntitiesAttributes)
                {
                    var entities = new List<Entity>();
                    foreach (var nestedEntity in nestedEntitiesAttributes)
                    {
                        try
                        {
                            entities.Add(ReadEntity(nestedEntity, metadata, jsonSerializer));
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                            // TODO: FixMe Ignore Parsing Errors
                        }
                    }

                    entity.Attributes.Add(attributeKey, entities);

                    toRemove.Add(attributeKey);
                    continue;
                }

                // Если Это вложенная сущность
                if (attributeValue is JObject expandedEntity)
                {
                    var relationship = metadata.GetRelationshipMetadata(
                        x => x.ReferencingEntity == entity.LogicalName
                             && (x.ReferencingAttribute == attributeKey ||
                                 x.ReferencingEntityNavigationPropertyName == attributeKey));

                    var nestedMd = metadata.GetEntityMetadata(x => x.LogicalName == relationship.ReferencedEntity);

                    var nestedEntity = expandedEntity.ToObject<Entity>(jsonSerializer);
                    if (nestedEntity != null)
                    {
                        nestedEntity.LogicalName = nestedMd.LogicalName;
                        if (nestedEntity.ContainsValue(nestedMd.PrimaryIdAttribute))
                        {
                            nestedEntity.Id = nestedEntity.GetAttributeValue<Guid>(nestedMd.PrimaryIdAttribute);
                        }

                        // Может уже лежать EntityReference на тот же аттрибут
                        entity.Attributes[attributeKey] = nestedEntity;

                        toRemove.Add(attributeKey);
                        continue;
                    }
                }

                // Если это свойства AliasedProperty
                if (attributeKey.Contains("_value", StringComparison.OrdinalIgnoreCase)
                    || attributeKey.Contains("_x002e_", StringComparison.OrdinalIgnoreCase)
                    || attributeKey.Contains("_x0040_", StringComparison.OrdinalIgnoreCase))
                {
                    var newName = FormatAttributeName(attributeKey);

                    entity.Attributes.Add(newName, attributeValue);

                    toRemove.Add(attributeKey);
                    continue;
                }

                // Иначе - перекладываем атрибуты
                entity.Attributes.Add(attributeKey, attributeValue);
                toRemove.Add(attributeKey);
            }

#if DEBUG
        var remainsKeys = attributes.Keys.Except(toRemove);
#endif
            return entity;
        }

        private static string FormatAttributeName(string name)
        {
            var newName = name;

            var matchValue = new Regex(@"^(_)(.+?)(_value)$").Match(name);

            if (matchValue.Success && matchValue.Groups.Count > 2)
            {
                newName = matchValue.Groups[2].Value;
            }

            newName = newName
                .Replace("_x002e_", ".", StringComparison.Ordinal)
                .Replace("_x0040_", "@", StringComparison.Ordinal);

            return newName;
        }

        private static string ParseFormatedValueProperty(in string name)
        {
            var newName = name.Replace("@OData.Community.Display.V1.FormattedValue", "", StringComparison.Ordinal);
            newName = FormatAttributeName(newName);
            //if (!string.IsNullOrWhiteSpace(newName) && !formatedValues.ContainsKey(newName))
            //{
            //    formatedValues.Add(newName, value);
            //}

            return newName;
        }

        private static bool IsFormatedValue(string name)
        {
            return (name ?? "").Contains("@OData.Community.Display.V1.FormattedValue", StringComparison.Ordinal);
        }

        public static bool TryParseCollectionName(string odataContext, out string collectionName)
        {
            collectionName = string.Empty;

            var splittedByEntity = (odataContext ?? string.Empty).Split("(");
            if (!splittedByEntity.Any())
            {
                return false;
            }

            collectionName = splittedByEntity[0]
                .Split("$metadata#")
                .LastOrDefault()
                .Split("/").First()
                .Split("(").First();

            return !string.IsNullOrEmpty(collectionName);
        }

        public static void EnsureSuccessStatusCode(HttpResponseMessage response, ILogger logger)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var errorData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var innerError = string.Empty;

            string message;
            if (response.Content.Headers.ContentType.MediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                message = errorData;
            }
            else if (response.Content.Headers.ContentType.MediaType.Equals("application/json",
                StringComparison.OrdinalIgnoreCase))
            {
                message = GetErrorData(errorData, out innerError);
            }
            else if (response.Content.Headers.ContentType.MediaType.Equals("text/html",
                StringComparison.OrdinalIgnoreCase))
            {
                message = $"HTML Error Content: {Environment.NewLine}{Environment.NewLine} {errorData}";
            }
            else
            {
                message =
                    $"Error occurred and no handler is available for content in the {response.Content.Headers.ContentType.MediaType} format.";
                innerError = errorData;
            }

            logger.LogError("Http {0}: Message:{1}", (int) response.StatusCode, new {message, innerError});
            throw new WebApiException(message) {StatusCode = response.StatusCode, InnerError = innerError};
        }

        private static string GetErrorData(string errorData, out string innerError)
        {
            innerError = String.Empty;
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jcontent = (JObject) JsonConvert.DeserializeObject(errorData, new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            });

            IDictionary<string, JToken> d = jcontent;

            if (d.TryGetValue("error", out var error))
            {
                var jsError = error as JObject;

                if (jsError.TryGetValue("innererror", out var innerEx))
                {
                    innerError = innerEx.ToString();
                }

                if (jsError.TryGetValue("message", out var message))
                {
                    return message.ToString();
                }

                return error.ToString();
            }
            else if (d.TryGetValue("Message", out var message))
            {
                return message.ToString();
            }

            return errorData;
        }
    }
}