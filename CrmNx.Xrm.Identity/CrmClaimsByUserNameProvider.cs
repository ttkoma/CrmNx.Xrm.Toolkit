using CrmNx.Xrm.Identity.Dto;
using CrmNx.Xrm.Identity.Internal;
using CrmNx.Xrm.Toolkit;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Identity
{
    public class CrmClaimsByUserNameProvider : ICrmClaimsProvider
    {
        private const string UserFields = "domainname,isdisabled";
        protected readonly ICrmWebApiClient CrmClient;
        protected Guid CurrentCrmUserId { get; private set; }

        public string ImpersonatedUserName { get; set; }

        public CrmClaimsByUserNameProvider(ICrmWebApiClient crmClient)
        {
            CrmClient = crmClient ?? throw new ArgumentNullException(nameof(crmClient));
        }

        public virtual async Task<IEnumerable<Claim>> GetCrmClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
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

            CurrentCrmUserId = crmUser.Id;

            var crmClaims = new List<Claim>
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

            // Roles claims
            var userRoles = await GetUserRolesAsync(CurrentCrmUserId, cancellationToken).ConfigureAwait(false);

            foreach (var role in userRoles)
            {
                crmClaims.Add(new Claim(
                    type: CrmClaimTypes.SystemUserRole,
                    value: role.Id.ToString(),
                    valueType: ClaimValueTypes.String,
                    issuer: CrmClaimTypes.Issuer));
            }

            // Priveleges Claims
            var userPrivileges = await GetUserPrivilegesAsync(CurrentCrmUserId, cancellationToken).ConfigureAwait(false);
            foreach (var privelege in userPrivileges)
            {
                crmClaims.Add(new Claim(
                    type: CrmClaimTypes.RolePrivelege,
                    value: privelege.PrivilegeId.ToString(),
                    valueType: ClaimValueTypes.String,
                    issuer: CrmClaimTypes.Issuer));
            }

            return crmClaims;
        }

        protected virtual async Task<ICrmSystemUser> FindFirstOrDefaultUser(string domainName)
        {
            var options = QueryOptions.Select(UserFields).Filter($"domainname eq '{domainName}'");

            EntityCollection collection;

            try
            {
                collection = await CrmClient
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

        protected virtual async Task<ICrmRole[]> GetUserRolesAsync(Guid systemuserId, CancellationToken cancellationToken)
        {
            var result = Array.Empty<Guid>();

            var userRolesFetchXml = FetchXml_UserRoles(systemuserId);
            var userRolesCollection = await CrmClient.RetrieveMultipleAsync(new FetchXmlExpression(userRolesFetchXml), cancellationToken)
                .ConfigureAwait(false);

            var teamsRolesFetchXml = FetchXml_UserTeamsRoles(systemuserId);
            var teamsRolesCollection = await CrmClient.RetrieveMultipleAsync(new FetchXmlExpression(teamsRolesFetchXml), cancellationToken)
                .ConfigureAwait(false);

            var rolesCollection = userRolesCollection.Entities
                .Union(teamsRolesCollection.Entities)
                .Distinct(new EntityIdComparer())
                .Select(x => x.ToEntity<CrmRole>());

            return rolesCollection.ToArray<ICrmRole>();
        }

        protected virtual async Task<RolePrivilege[]> GetUserPrivilegesAsync(Guid systemuserId, CancellationToken cancellationToken) 
        {
            var request = new RetrieveUserPrivilegesRequest(systemuserId);
            var response = await CrmClient.ExecuteAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return response.RolePrivileges.ToArray();
        }

        private static string FetchXml_UserRoles(Guid systemuserId)
        {
            var fetchData = new
            {
                componentstate = (int)CrmRole.ComponentStateEnum.Published,
                systemuserid = systemuserId
            };

            var fetchXml = $@"
            <fetch distinct='true' no-lock='true' latematerialize='true'>
              <entity name='{CrmRole.EntityLogicalName}'>
                <attribute name='{CrmRole.PropertyNames.Name}' />
                <attribute name='{CrmRole.PrimaryIdAttribute}' />
                <filter type='and'>
                  <condition attribute='{CrmRole.PropertyNames.ComponentState}' operator='eq' value='{fetchData.componentstate}'/>
                  <condition entityname='systemuserroles' attribute='systemuserid' operator='eq' value='{fetchData.systemuserid}' />
                </filter>
                <link-entity name='systemuserroles' from='roleid' to='roleid' />
              </entity>
            </fetch>";

            return fetchXml;
        }

        private static string FetchXml_UserTeamsRoles(Guid systemuserId)
        {
            var fetchData = new
            {
                componentstate = (int)CrmRole.ComponentStateEnum.Published,
                systemuserid = systemuserId
            };

            var fetchXml = $@"
            <fetch distinct='true' no-lock='true' latematerialize='true'>
              <entity name='{CrmRole.EntityLogicalName}'>
                <attribute name='{CrmRole.PropertyNames.Name}' />
                <attribute name='{CrmRole.PrimaryIdAttribute}' />
                <filter type='and'>
                  <condition attribute='{CrmRole.PropertyNames.ComponentState}' operator='eq' value='{fetchData.componentstate}'/>
                  <condition entityname='teammembership' attribute='systemuserid' operator='eq' value='{fetchData.systemuserid}' />
                </filter>
                <link-entity name='teamroles' from='roleid' to='roleid'>
                  <attribute name='roleid' />
                  <link-entity name='teammembership' from='teamid' to='teamid' />
                </link-entity>
              </entity>
            </fetch>";


            return fetchXml;
        }
    }
}