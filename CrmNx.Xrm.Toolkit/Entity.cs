using System;
using System.Collections.Generic;
using System.Globalization;

namespace CrmNx.Xrm.Toolkit
{
    public class Entity
    {
        public Dictionary<string, object> Attributes { get; }
        public Dictionary<string, string> FormattedValues { get; }
        public Dictionary<string, object> KeyAttributes { get; }

        public string LogicalName { get; set; }

        //public string EntitySetName { get; set; }

        public string ETag { get; set; }

        public Guid Id { get; set; }

        public Entity(Entity otherEntity)
        {
            if (otherEntity == null)
                throw new ArgumentNullException(nameof(otherEntity));

            Id = otherEntity.Id;
            LogicalName = otherEntity.LogicalName;
            // TODO: Реализовать копирование значений
            Attributes = otherEntity.Attributes;
            FormattedValues = otherEntity.FormattedValues;
            KeyAttributes = otherEntity.KeyAttributes;
        }

        /// <summary>
        /// Создаёт новый экземпляр сущности
        /// </summary>
        public Entity()
        {
            Attributes ??= new Dictionary<string, object>();

            FormattedValues ??= new Dictionary<string, string>();

            KeyAttributes ??= new Dictionary<string, object>();
        }

        /// <summary>
        /// Создаёт новый экземпляр сущности
        /// </summary>
        /// <param name="logicalName">Логическое имя сущности</param>
        public Entity(string logicalName) : this()
        {
            LogicalName = logicalName;
        }

        /// <summary>
        /// Создаёт новый экземпляр сущности
        /// </summary>
        /// <param name="logicalName">Логическое имя сущности</param>
        /// <param name="id">Идентификатор сущсности</param>
        public Entity(string logicalName, Guid id) : this()
        {
            LogicalName = logicalName;
            Id = id;
        }

        /// <summary>
        /// Создаёт новый экземпляр сущности
        /// </summary>
        /// <param name="logicalName">Логическое имя сущности</param>
        /// <param name="keyName">Имя ключа</param>
        /// <param name="keyValue">Значение ключа</param>
        public Entity(string logicalName, string keyName, object keyValue) : this()
        {
            LogicalName = logicalName;
            KeyAttributes.Clear();
            KeyAttributes.Add(keyName, keyValue);
        }

        /// <summary>
        /// Создаёт новый экземпляр сущности
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="keyAttributes">Задает коллекцию ключевых атрибутов.</param>
        public Entity(string logicalName, Dictionary<string, object> keyAttributes) : this()
        {
            LogicalName = logicalName;
            KeyAttributes = keyAttributes;
        }

        public object this[string index]
        {
            get
            {
                if (Contains(index))
                {
                    return Attributes[index];
                }

                return null;
            }
            set
            {
                Attributes[index] = value;
            }
        }

        public T GetAttributeValue<T>(string atributeName)
        {
            if (!Contains(atributeName))
            {
                return default(T);
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)Convert.ToInt32(Attributes[atributeName], CultureInfo.InvariantCulture);
            }

            if ((typeof(DateTime) == typeof(T) || typeof(DateTime?) == typeof(T)) && Attributes[atributeName] is string)
            {
                return (T)(object)Convert.ToDateTime(Attributes[atributeName], CultureInfo.InvariantCulture);
            }

            if (typeof(Guid) == typeof(T) && Attributes[atributeName] is string rawValue)
            {
                return (T)(object)new Guid(rawValue);
            }

            return (T)Attributes[atributeName];
        }

        /// <summary>
        /// Проверяет наличие аттрибута
        /// </summary>
        /// <param name="atributeName">Имя атрибута</param>
        /// <returns></returns>
        public bool Contains(string atributeName)
        {
            return !string.IsNullOrWhiteSpace(atributeName) && Attributes.ContainsKey(atributeName);
        }

        /// <summary>
        /// Проверяет наличие аттрибута с заполненным значением
        /// </summary>
        /// <param name="atributeName">Имя атрибута</param>
        /// <returns></returns>
        public bool ContainsValue(string atributeName)
        {
            if (!Contains(atributeName))
            {
                return false;
            }

            return Attributes[atributeName] != null;
        }

    }
}
