using System;
using System.Security.Claims;
using CrmNx.Xrm.Identity.Internal;

namespace CrmNx.Xrm.Identity
{
    public static class ClaimsIdentityExtensions
    {
        /// <summary>
        /// Return Crm system user ID from claims
        /// </summary>
        /// <param name="principal">Authenticated user claims</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a value that indicates whether the entity (user) represented by this claims principal is in the specified Crm role.
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="crmRoleId">The role for which to check.</param>
        /// <returns>true if claims principal is in the specified Crm role; otherwise, false.</returns>
        public static bool HasCrmRole(this ClaimsPrincipal principal, Guid crmRoleId)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.HasClaim(CrmClaimTypes.SystemUserRole, crmRoleId.ToString());
        }

        /// <summary>
        /// Returns a value that indicates whether the entity (user) represented by this claims principal is in the specified Crm roleprivilege.
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="crmPrivilegeId">The roleprivilege for which to check.</param>
        /// <returns>true if claims principal is in the specified Crm roleprivilege; otherwise, false.</returns>
        public static bool HasCrmPrivilege(this ClaimsPrincipal principal, Guid crmPrivilegeId)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.HasClaim(CrmClaimTypes.RolePrivelege, crmPrivilegeId.ToString());
        }
    }
}