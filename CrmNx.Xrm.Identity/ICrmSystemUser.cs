using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Identity
{
    public interface ICrmSystemUser
    {
        Guid Id { get; set; }

        bool IsDisabled { get; set; }
    }
}