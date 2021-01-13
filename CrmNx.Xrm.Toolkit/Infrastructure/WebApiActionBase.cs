using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public abstract class WebApiActionBase : IWebApiAction
    {
        public EntityReference BoundEntity { get; }
        public string Action { get; }
        
        public IDictionary<string, object> Parameters { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Default constructor for unbound Action
        /// </summary>
        /// <param name="action">Full action name (or path query)</param>
        protected WebApiActionBase(string action)
        {
            Action = action ?? throw new ArgumentException("Action cannot be empty.", nameof(action));
        }

        /// <summary>
        /// Default constructor for bound Action
        /// </summary>
        /// <param name="boundEntity">Bound entity reference</param>
        /// <param name="action">Full action name (or path query)</param>
        protected WebApiActionBase(EntityReference boundEntity, string action) : this(action)
        {
            BoundEntity = boundEntity ?? throw new ArgumentNullException(nameof(boundEntity));
        }
    }
}