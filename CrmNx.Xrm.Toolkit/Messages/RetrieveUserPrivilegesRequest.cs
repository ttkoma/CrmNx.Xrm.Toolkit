using System;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class RetrieveUserPrivilegesRequest : OrganizationRequest<RetrieveUserPrivilegesResponse>
    {
        private const string WebApiFunctionName = "Microsoft.Dynamics.CRM.RetrieveUserPrivileges";

        public RetrieveUserPrivilegesRequest(Guid userId)
            : base(WebApiFunctionName, false)
        {
            UserId = userId;
        }

        public Guid UserId { get; set; }

        public override string RequestBindingPath => $"systemusers({UserId})";

        //public override string QueryString()
        //{
        //    return $"systemusers({_systemUserId})/{FunctionName}";
        //}
    }
}