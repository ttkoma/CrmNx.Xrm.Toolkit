using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class RetrieveUserPrivilegesResponse
    {
        /// <summary>
        ///     Contains information about a privilege.
        /// </summary>
        public IEnumerable<RolePrivilege> RolePrivileges;
    }

    /// <summary>
    ///     Contains information about a privilege.
    /// </summary>
    public class RolePrivilege
    {
        /// <summary>
        ///     The ID of the privilege.
        /// </summary>
        public Guid PrivilegeId { get; set; }

        /// <summary>
        ///     The ID of the business unit.
        /// </summary>
        public Guid BusinessUnitId { get; set; }

        /// <summary>
        ///     The depth of the privilege.
        /// </summary>
        public PrivilegeDepth Depth { get; set; }
    }
}