using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Xrm.Identity.Dto;
using CrmNx.Xrm.Identity.Internal;
using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Query;

namespace CrmNx.Xrm.Identity
{
    public class CrmClaimsByUserNameProvider : ICrmClaimsProvider
    {
        private const string UserFields = "domainname,isdisabled";
        private readonly ICrmWebApiClient _crmClient;

        public string ImpersonatedUserName { get; set; }

        public CrmClaimsByUserNameProvider(ICrmWebApiClient crmClient)
        {
            _crmClient = crmClient ?? throw new ArgumentNullException(nameof(crmClient));
        }

        public virtual async Task<IEnumerable<Claim>> GetCrmClaimsAsync(ClaimsPrincipal principal)
        {
            var userName = ImpersonatedUserName;

            if (string.IsNullOrEmpty(userName))
            {
                userName = principal.Identity.Name;
            }

            if (string.IsNullOrEmpty(userName))
            {
                return default;
            }

            var crmUser = await FindFirstOrDefaultUser(userName).ConfigureAwait(false);

            if (crmUser == null)
            {
                return default;
            }

            var crmClaims = new[]
            {
                new Claim(
                    type: CrmClaimTypes.SystemUserId,
                    value: crmUser.Id.ToString(),
                    valueType: ClaimValueTypes.String,
                    issuer: CrmClaimTypes.Issuer
                ),

                new Claim(
                    type: CrmClaimTypes.SystemUserActive,
                    value: (!crmUser.IsDisabled).ToString(CultureInfo.InvariantCulture),
                    valueType: ClaimValueTypes.Boolean,
                    issuer: CrmClaimTypes.Issuer
                )
            };

            return crmClaims;
        }

        protected virtual async Task<ICrmSystemUser> FindFirstOrDefaultUser(string domainName)
        {
            var options = QueryOptions.Select(UserFields).Filter($"domainname eq '{domainName}'");

            EntityCollection collection;

            try
            {
                collection = await _crmClient
                    .RetrieveMultipleAsync("systemuser", options, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (WebApiException webEx)
            {
                if (webEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Error S2S Crm authorization. Check service credentials.");
                }

                throw;
            }

            return collection.Entities.Any() ? collection.Entities.FirstOrDefault().ToEntity<CrmSystemUser>() : default;
        }
    }
}