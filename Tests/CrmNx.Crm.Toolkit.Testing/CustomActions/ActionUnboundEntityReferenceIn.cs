using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Messages;

namespace CrmNx.Crm.Toolkit.Testing.CustomActions
{
    public class ActionUnboundEntityReferenceIn: OrganizationRequest<object>
    {
        public const string ActionName = "new_XrmToolkitTestActionUnbound";
        public const bool IsAction = true;

        /// <summary>
        /// Ne Action Request
        /// </summary>
        /// <param name="userRef">Required parameter</param>
        public ActionUnboundEntityReferenceIn(EntityReference userRef)
            : base(ActionName, IsAction)
        {
            UserRef = userRef;
            AnyRef = null;
        }

        public EntityReference UserRef
        {
            get => Parameters[nameof(UserRef)] as EntityReference;
            set => Parameters[nameof(UserRef)] = value;
        }
        
        public EntityReference AnyRef
        {
            get => Parameters[nameof(AnyRef)] as EntityReference;
            set => Parameters[nameof(AnyRef)] = value;
        }
    }
}