using System;
using CrmNx.Xrm.Toolkit;

namespace CrmNx.Crm.Toolkit.Testing.ProxyClasses
{
    public class Account : Entity
    {
        public const string EntityLogicalName = "account";
        public const string PrimaryIdAttributeName = "accountid";

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Account() : base(EntityLogicalName)
        {
        }

        public Account(Guid accountId) : this()
        {
            Id = accountId;
        }

        public static EntityReference Reference(Guid id) => new EntityReference(EntityLogicalName, id);


        public Account(int accountNumber) : base(EntityLogicalName, PropertyNames.AccountNumber, accountNumber)
        {
        }


        public Account(Entity otherEntity) : base(otherEntity)
        {
            if (otherEntity == null) throw new ArgumentNullException(nameof(otherEntity));

            LogicalName = EntityLogicalName;
            Id = otherEntity.Id;
        }

        public sealed override Guid Id
        {
            get => GetAttributeValue<Guid>(PrimaryIdAttributeName);
            set => SetAttributeValue(PrimaryIdAttributeName, value);
        }


        public int AccountNumber
        {
            get => GetAttributeValue<int>(PropertyNames.AccountNumber);
            set => SetAttributeValue(PropertyNames.AccountNumber, value);
        }
        
        public EntityReference PrimaryContactId
        {
            get => GetAttributeValue<EntityReference>(PropertyNames.PrimaryContactId);
            set => SetAttributeValue(PropertyNames.PrimaryContactId, value);
        }

        public StateCodeEnum StateCode
        {
            get => GetAttributeValue<StateCodeEnum>(PropertyNames.StateCode);
            set => SetAttributeValue(PropertyNames.StateCode, value);
        }

        public enum StateCodeEnum
        {
            Active = 0,
            InActive = 1
        }

        /// <summary>
        /// Атрибуты сущности
        /// </summary>
        public static class PropertyNames
        {

            public const string StateCode = "statecode";
            public const string AccountNumber = "accountnumber";
            public const string PrimaryContactId = "primarycontactid";
        }
    }
}
