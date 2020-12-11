using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    /// Adds members to a team.
    /// </summary>
    public class AddMembersTeamAction : WebApiActionBase
    {
        private const string ActionName = "Microsoft.Dynamics.CRM.AddMembersTeam";
        private const string MembersParameterName = "Members";

        public IList<EntityReference> Members => Parameters[MembersParameterName] as IList<EntityReference>;

        /// <summary>
        /// Create action instance
        /// </summary>
        /// <param name="team">The team to which members will be added</param>
        public AddMembersTeamAction(EntityReference team) : base(team, ActionName)
        {
            var membersList = new List<EntityReference>();
            
            Parameters.Add(MembersParameterName, membersList);
        }
    }
}