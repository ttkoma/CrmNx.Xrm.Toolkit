using CrmNx.Xrm.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrmNx.Xrm.Identity.Dto
{
    /// <summary>
    /// <inheritdoc cref="ICrmRole"/>
    /// </summary>
    public class CrmRole : Entity, ICrmRole
    {
        public const string EntityLogicalName = "role";
        public const string PrimaryIdAttribute = "roleid";

        public CrmRole() : base(EntityLogicalName)
        {
        }

        /// <inheritdoc />
        public new Guid Id
        {
            get => GetAttributeValue<Guid>(PrimaryIdAttribute);
            set
            {
                base.Id = value;
                SetAttributeValue(PrimaryIdAttribute, value);
            }
        }

        /// <inheritdoc />
        public string Name
        {
            get => GetAttributeValue<string>(PropertyNames.Name);
            set => SetAttributeValue(PropertyNames.Name, value);
        }

        public ComponentStateEnum ComponentState
        {
            get => GetAttributeValue<ComponentStateEnum>(PropertyNames.ComponentState);
            set => SetAttributeValue(PropertyNames.ComponentState, value);
        }

        public enum ComponentStateEnum
        {
            Published = 0,
            Unpublished = 1,
            Deleted = 2,
            DeletedUnpublished = 3
        }

        public static class PropertyNames
        {
            public const string Name = "name";
            public const string ComponentState = "componentstate";
        }
    }
}
