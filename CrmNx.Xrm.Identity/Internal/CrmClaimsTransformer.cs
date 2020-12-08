using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace CrmNx.Xrm.Identity.Internal
{
    internal class CrmClaimsTransformer : IClaimsTransformation
    {
        private readonly ICrmClaimsProvider _crmClaimsProvider;

        public CrmClaimsTransformer(ICrmClaimsProvider crmClaimsProvider)
        {
            _crmClaimsProvider = crmClaimsProvider ?? throw new ArgumentNullException(nameof(crmClaimsProvider));
        }

        /// <summary>
        /// Add Crm Claims for authenticated user
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var crmClaims = await _crmClaimsProvider.GetCrmClaimsAsync(principal);

            if (crmClaims != null)
            {
                ((ClaimsIdentity) principal.Identity).AddClaims(crmClaims);
            }
            else
            {
            }

            return principal;
        }
    }
}