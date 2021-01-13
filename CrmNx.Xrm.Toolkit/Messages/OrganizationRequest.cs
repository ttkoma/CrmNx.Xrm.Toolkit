using System;
using System.Collections.Generic;
using System.Text;
using CrmNx.Xrm.Toolkit.Infrastructure;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class OrganizationRequest<TResponse>
    {
        public OrganizationRequest(string requestName, bool isWebApiAction = false)
        {
            RequestName = requestName;
            Parameters = new Dictionary<string, object>();

            IsWebApiAction = isWebApiAction;
        }

        public OrganizationRequest() : this(string.Empty, false) {}

        public string RequestName { get; set; }

        public Guid? RequestId { get; set; }

        public bool IsWebApiAction { get; set; }

        public IDictionary<string, object> Parameters { get; }

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

            if (!IsWebApiAction && !string.IsNullOrEmpty(RequestName))
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