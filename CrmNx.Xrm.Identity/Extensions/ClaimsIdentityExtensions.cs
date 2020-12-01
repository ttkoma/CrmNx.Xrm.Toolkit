using System;
using System.Security.Claims;

namespace CrmNx.Xrm.Identity
{
    public static class ClaimsIdentityExtensions
    {
        public static Guid CrmUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var crmUserIdClaim = principal.FindFirst(c => c.Type == CrmClaimTypes.SystemUserId &&
                    c.Issuer == CrmClaimTypes.Issuer);

            return crmUserIdClaim == null ? default : Guid.Parse(crmUserIdClaim.Value);
        }
    }
}
