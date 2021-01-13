using System;
using System.Collections.Generic;
using System.Text;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public abstract class WebApiFunctionBase : IWebApiFunction
    {
        public EntityReference BoundEntity { get; set; }

        public string RequestName { get; }

        public IDictionary<string, object> Parameters { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Default constructor for unbound Function
        /// </summary>
        /// <param name="function">Full function name (or path query)</param>
        protected WebApiFunctionBase(string function)
        {
            RequestName = function ?? throw new ArgumentException("Function cannot be empty.", nameof(function));
        }

        /// <summary>
        /// Default constructor for bound Function
        /// </summary>
        /// <param name="boundEntity">Bound entity reference</param>
        /// <param name="functionName">Full function name (or path query)</param>
        protected WebApiFunctionBase(EntityReference boundEntity, string functionName) : this(functionName)
        {
            BoundEntity = boundEntity ?? throw new ArgumentNullException(nameof(boundEntity));
        }
    }
}
