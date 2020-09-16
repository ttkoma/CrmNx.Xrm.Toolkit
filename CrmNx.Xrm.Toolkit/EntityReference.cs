using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit
{
    public class EntityReference
    {
        public Guid Id { get; set; }

        public string LogicalName { get; set; }

        public Dictionary<string, object> KeyAttributes { get; } = new Dictionary<string, object>();

        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the EntityReference class.
        /// </summary>
        /// <param name="logicalName">Specifies the entity logical name.</param>
        /// <param name="id">The ID of the record.</param>
        public EntityReference(string logicalName, Guid id) : this(logicalName)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the EntityReference class.
        /// </summary>
        /// <param name="logicalName">Specifies the entity logical name.</param>
        public EntityReference(string logicalName)
        {
            LogicalName = logicalName;
        }

        /// <summary>
        /// Initializes a new instance of the EntityReference class.
        /// </summary>
        public EntityReference() { }

        public EntityReference(string logicalName, string keyName, object keyValue)
        {
            LogicalName = logicalName;
            KeyAttributes = new Dictionary<string, object>
            {
                { keyName, keyValue }
            };
        }

        public EntityReference(string logicalName, Dictionary<string, object> keyAttributes)
        {
            LogicalName = logicalName;
            KeyAttributes = keyAttributes;
        }
    }
}
