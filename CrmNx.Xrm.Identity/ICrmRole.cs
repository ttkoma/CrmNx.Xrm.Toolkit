using System;
using System.Collections.Generic;
using System.Text;

namespace CrmNx.Xrm.Identity
{
    /// <summary>
    /// Crm role
    /// </summary>
    public interface ICrmRole
    {
        /// <summary>
        /// Role Id
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Role Name
        /// </summary>
        string Name { get; set; }
    }
}
