using CrmNx.Xrm.Identity.Dto;
using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Query;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace CrmNx.Xrm.Identity
{
    public class CrmClaimsTransformer : IClaimsTransformation
    {
        private readonly ICrmWebApiClient _crmClient;
        private const string UserFields = "domainname,isdisabled";

        public CrmClaimsTransformer(ICrmWebApiClient crmClient)
        {
            _crmClient = crmClient ?? throw new ArgumentNullException(nameof(crmClient));
        }

        /// <summary>
        /// Выполняет преобразование Claims добавляя утверждения на основе учетной записи CRM.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var nameClaim = ((ClaimsIdentity)principal.Identity).FindFirst(c => c.Type == ClaimTypes.Name);

            if (nameClaim == null)
            {
                return principal;
            }

            var crmUser = await FindFirstOrDefaultUser(nameClaim.Value).ConfigureAwait(false);

            if (crmUser == null)
            {
                return principal;
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

            ((ClaimsIdentity)principal.Identity).AddClaims(crmClaims);

            return principal;
        }

        private async Task<SystemUserDto> FindFirstOrDefaultUser(string domainName)
        {
            var options = QueryOptions.Select(columns: UserFields)
                .Filter($"domainname eq '{domainName}'");

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

            if (collection.Entities.Any())
            {
                return collection.Entities.FirstOrDefault().ToEntity<SystemUserDto>();
            }

            return default;
        }
    }
}