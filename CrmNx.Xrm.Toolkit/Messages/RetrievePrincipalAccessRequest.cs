using System;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class RetrievePrincipalAccessRequest : OrganizationRequest<RetrievePrincipalAccessResponse>
    {
        private const string WebApiFunctionName = "Microsoft.Dynamics.CRM.RetrievePrincipalAccess";

        /// <summary>
        ///     Retrieves the access rights of the specified security principal (team or user) to the specified record.
        /// </summary>
        /// <param name="principal">Security principal (team or user)</param>
        /// <param name="target">The target record for which to retrieve access rights.</param>
        public RetrievePrincipalAccessRequest(EntityReference principal, EntityReference target) : base(
            WebApiFunctionName, false)
        {
            Target = target;
            Principal = principal;
        }

        public override string RequestBindingPath
        {
            get
            {
                return Principal?.LogicalName.ToLowerInvariant() switch
                {
                    "team" => $"teams({Principal.Id})",
                    "systemuser" => $"systemusers({Principal.Id})",
                    _ => throw new ArgumentOutOfRangeException(nameof(Principal.LogicalName), Principal?.LogicalName,
                        $"Unsupported value entity type of principal. Available values: 'systemuser','team', actual value: '{Principal?.LogicalName.ToLowerInvariant()}'")
                };
            }
        }

        public EntityReference Principal { get; set; }

        public EntityReference Target
        {
            get => Parameters.ContainsKey(nameof(Target))
                ? (EntityReference) Parameters[nameof(Target)]
                : default;
            set => Parameters[nameof(Target)] = value;
        }
    }
}