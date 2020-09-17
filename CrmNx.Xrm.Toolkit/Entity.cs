using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrmNx.Xrm.Toolkit
{
    /// <summary>
    /// Dynamics Entity
    /// </summary>
    public class Entity
    {
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
        public Dictionary<string, string> FormattedValues { get; } = new Dictionary<string, string>();
        public Dictionary<string, object> KeyAttributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Entity LogicalName
        /// </summary>
        public string LogicalName { get; set; }

        public string ETag { get; set; }

        /// <summary>
        /// Entity unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Initialize new Entity instance from another entity
        /// </summary>
        /// <param name="otherEntity">Other entity</param>
        /// <exception cref="ArgumentNullException">When other entity is null</exception>
        public Entity(Entity otherEntity)
        {
            if (otherEntity == null)
                throw new ArgumentNullException(nameof(otherEntity));

            Id = otherEntity.Id;
            LogicalName = otherEntity.LogicalName;

            Attributes = new Dictionary<string, object>(otherEntity.Attributes);
            FormattedValues = new Dictionary<string, string>(otherEntity.FormattedValues);
            KeyAttributes = new Dictionary<string, object>(otherEntity.KeyAttributes);
        }

        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        public Entity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EntityReference class.
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        public Entity(string logicalName) : this()
        {
            LogicalName = logicalName;
        }

        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <param name="id">Unique identifier</param>
        public Entity(string logicalName, Guid id) : this()
        {
            LogicalName = logicalName;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <param name="keyName">Alternate Key name</param>
        /// <param name="keyValue">Alternate Key value</param>
        public Entity(string logicalName, string keyName, object keyValue) : this()
        {
            LogicalName = logicalName;
            KeyAttributes.Add(keyName, keyValue);
        }

        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <param name="keyAttributes">Collection of alternate keys with values</param>
        public Entity(string logicalName, Dictionary<string, object> keyAttributes) : this()
        {
            LogicalName = logicalName;
            KeyAttributes = keyAttributes;
        }

        public object this[string attributeName]
        {
            get => Contains(attributeName) ? Attributes[attributeName] : null;
            set => Attributes[attributeName] = value;
        }

        public T GetAttributeValue<T>(string attributeName)
        {
            // Get base T from Nullable<T>
            var t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (!Attributes.ContainsKey(attributeName)) return default;

            var value = Attributes[attributeName];
            if (value == null) return default;

            if (value.GetType() == typeof(T)) return (T) value;

            try
            {
                object safeValue = null;
                var converter = TypeDescriptor.GetConverter(typeof(T));

                if (converter.CanConvertFrom(value.GetType()))
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    safeValue = (T) converter.ConvertFrom(value);
                }
                else if (TypeDescriptor.GetConverter(value.GetType()).CanConvertTo(typeof(T)))
                {
                    safeValue = TypeDescriptor.GetConverter(value.GetType()).ConvertTo(value, typeof(T));
                }

                return (T) safeValue;
            }
            catch (Exception)
            {
                throw new InvalidCastException(
                    $"Cannot convert field '{attributeName}' with Type '{value.GetType().FullName}' to Type '{typeof(T).FullName}'",
                    2001);
            }


            // if (!Contains(attributeName))
            // {
            //     return default(T);
            // }
            //
            // if (typeof(T) == typeof(int))
            // {
            //     return (T)(object)Convert.ToInt32(Attributes[attributeName], CultureInfo.InvariantCulture);
            // }
            //
            // if ((typeof(DateTime) == typeof(T) || typeof(DateTime?) == typeof(T)) && Attributes[attributeName] is string)
            // {
            //     return (T)(object)Convert.ToDateTime(Attributes[attributeName], CultureInfo.InvariantCulture);
            // }
            //
            // if (typeof(Guid) == typeof(T) && Attributes[attributeName] is string rawValue)
            // {
            //     return (T)(object)new Guid(rawValue);
            // }
            //
            // return (T)Attributes[attributeName];
        }

        public void SetAttributeValue(string attributeName, object value)
        {
            Attributes[attributeName] = value;
        }

        public bool Contains(string attributeName)
        {
            return !string.IsNullOrWhiteSpace(attributeName) && Attributes.ContainsKey(attributeName);
        }

        public bool ContainsValue(string attributeName)
        {
            if (!Contains(attributeName))
            {
                return false;
            }

            return Attributes[attributeName] != null;
        }
    }
}