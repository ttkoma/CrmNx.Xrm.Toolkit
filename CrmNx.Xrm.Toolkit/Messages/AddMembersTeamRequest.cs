using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Adds members to a team.
    /// </summary>
    public class AddMembersTeamRequest : OrganizationRequest<object>
    {
        private const string WebApiActionName = "Microsoft.Dynamics.CRM.AddMembersTeam";

        /// <summary>
        ///     Create action instance
        /// </summary>
        /// <param name="teamId">The team to which members will be added</param>
        public AddMembersTeamRequest(Guid teamId) : base(WebApiActionName, true)
        {
            TeamId = teamId;

            var membersList = new List<EntityReference>();

            Parameters.Add(nameof(Members), membersList);
        }

        public List<EntityReference> Members => Parameters[nameof(Members)] as List<EntityReference>;

        public Guid TeamId { get; set; }

        public override string RequestBindingPath => $"teams({TeamId})";
    }
}