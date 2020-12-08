using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Identity
{
    public interface ICrmClaimsProvider
    {
        Task<IEnumerable<Claim>> GetCrmClaimsAsync(ClaimsPrincipal principal);
    }
}