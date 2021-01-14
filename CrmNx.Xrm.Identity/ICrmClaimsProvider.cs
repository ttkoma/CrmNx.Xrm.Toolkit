using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Identity
{
    public interface ICrmClaimsProvider
    {
        Task<IEnumerable<Claim>> GetCrmClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);

        Guid[] ApplicationRoles { get; set; }

        bool UseRoles { get; set; }

        Guid[] ApplicationPrivileges { get; set; }

        bool UseRolePrivileges { get; set; }
    }
}