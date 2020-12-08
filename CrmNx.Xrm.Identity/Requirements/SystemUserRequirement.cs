using Microsoft.AspNetCore.Authorization;

namespace CrmNx.Xrm.Identity.Requirements
{
    public sealed class SystemUserRequirement : IAuthorizationRequirement
    {
        public SystemUserRequirement()
        {
        }
    }
}