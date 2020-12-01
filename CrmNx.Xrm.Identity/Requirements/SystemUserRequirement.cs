using Microsoft.AspNetCore.Authorization;

namespace CrmNx.Xrm.Identity.Requirements
{
    public class SystemUserRequirement : IAuthorizationRequirement
    {
        public SystemUserRequirement()
        {
        }
    }
}
