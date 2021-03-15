using System;
using System.Collections.Generic;
using System.Text;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class OrganizationRequest<TResponse>
    {
        /// <summary>
        /// Create WebApi request with Function/Action name 
        /// </summary>
        /// <param name="requestName">WebApi Action or Function name</param>
        /// <param name="isWebApiAction">Is WebApi Action</param>
        public OrganizationRequest(string requestName, bool isWebApiAction = false)
        {
            RequestName = requestName;
            Parameters = new Dictionary<string, object>();

            IsWebApiAction = isWebApiAction;
        }

        /// <summary>
        /// Create WebApi request as WebApi Function
        /// </summary>
        public OrganizationRequest() : this(string.Empty, false) { }

        /// <summary>
        /// WebApi Function or Action Name
        /// </summary>
        public string RequestName { get; set; }

        public Guid? RequestId { get; set; }

        /// <summary>
        /// Is WebApi Action request
        /// </summary>
        public bool IsWebApiAction { get; set; }

        public IDictionary<string, object> Parameters { get; }

        /// <summary>
        /// WebApi Request parts (e.g. entity set name)
        /// </summary>
        public virtual string RequestBindingPath { get; set; } = string.Empty;

        public virtual string RequestPath()
        {
            var requestSegments = new List<string>();
            if (!string.IsNullOrEmpty(RequestBindingPath))
                requestSegments.Add(RequestBindingPath);

            if (!string.IsNullOrEmpty(RequestName))
                requestSegments.Add(RequestName);

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendJoin("/", requestSegments);

            if (IsWebApiAction)
            {
                return queryBuilder.ToString();
            }
            else if (!string.IsNullOrEmpty(RequestName))
            {
                var paramsList = new List<string>();
                foreach (var (key, _) in Parameters)
                {
                    paramsList.Add($"{key}=@{key}");
                }
                queryBuilder.Append("(");
                queryBuilder.AppendJoin(",", paramsList);
                queryBuilder.Append(")");
            }

            return queryBuilder.ToString();
        }


    }
}