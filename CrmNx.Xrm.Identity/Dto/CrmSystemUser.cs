using System;
using CrmNx.Xrm.Toolkit;

namespace CrmNx.Xrm.Identity.Dto
{
    public class CrmSystemUser : Entity, ICrmSystemUser
    {
        public const string EntityLogicalName = "systemuser";
        public const string PrimaryIdAttribute = "systemuserid";

        public CrmSystemUser() : base(EntityLogicalName)
        {
        }

        public sealed override Guid Id
        {
            get => GetAttributeValue<Guid>(PrimaryIdAttribute);
            set => SetAttributeValue(PrimaryIdAttribute, value);
        }

        public string DomainName
        {
            get => GetAttributeValue<string>(PropertyNames.DomainName);
            set => SetAttributeValue(PropertyNames.DomainName, value);
        }

        public string FullName
        {
            get => GetAttributeValue<string>(PropertyNames.FullName);
            set => SetAttributeValue(PropertyNames.FullName, value);
        }

        public bool IsDisabled
        {
            get => GetAttributeValue<bool>(PropertyNames.IsDisabled);
            set => SetAttributeValue(PropertyNames.IsDisabled, value);
        }

        public static class PropertyNames
        {
            public const string DomainName = "domainname";
            public const string FullName = "fullname";
            public const string IsDisabled = "isdisabled";
        }
    }
}