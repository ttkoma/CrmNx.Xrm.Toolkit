using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit
{
    public class EntityReference
    {
        /// <summary>
        /// Unique identifier Entity
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Entity LogicaName
        /// </summary>
        public string LogicalName { get; set; }

        /// <summary>
        /// Collection of KeyValue pair contains alternate key and value
        /// </summary>
        public Dictionary<string, object> KeyAttributes { get; } = new Dictionary<string, object>();

        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the EntityReference class.
        /// </summary>
        /// <param name="logicalName">Specifies the entity logical name.</param>
        /// <param name="id">The ID of the entity.</param>
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
        /// Initialize new instance of the EntityReference form a alternate Key and Value
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <param name="keyName">Alternate key name</param>
        /// <param name="keyValue">Alternate key value</param>
        public EntityReference(string logicalName, string keyName, object keyValue)
        {
            LogicalName = logicalName;
            KeyAttributes = new Dictionary<string, object>
            {
                {keyName, keyValue}
            };
        }

        /// <summary>
        /// Initialize new instance of the EntityReference form a dictionary alternate Keys and Values
        /// </summary>
        /// <param name="logicalName">Entity LogicalName</param>
        /// <param name="keyAttributes">Collection of KeyValue pair contains alternate key and value</param>
        public EntityReference(string logicalName, IDictionary<string, object> keyAttributes)
        {
            LogicalName = logicalName;
            KeyAttributes = new Dictionary<string, object>(keyAttributes);
        }
    }
}