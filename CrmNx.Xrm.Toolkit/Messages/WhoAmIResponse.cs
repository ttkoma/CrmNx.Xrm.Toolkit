using System;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class WhoAmIResponse
    {
        public Guid UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public Guid BusinessUnitId { get; set; }
    }
}