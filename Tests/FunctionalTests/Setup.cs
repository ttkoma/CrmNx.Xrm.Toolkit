using System;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class Setup : Crm.Toolkit.Testing.SetupBase
    {
        public Guid OrganizationId { get; set; } = new Guid("40acdadc-0a7c-e611-80bf-005056b42933");

        public string HouseFiasGuid { get; set; } = "9d5df7ae-a99c-4b3e-843c-fa3fd776910c";

        public Guid PrimaryContactId { get; set; } = new Guid("8F1E0D99-71F8-EA11-AADD-005056B427FF");

        /// <summary>
        /// Bias from TimeZoneDefinitions for User at executing tests
        /// </summary>
        public TimeSpan TesterUserTimeZoneOffset => TimeSpan.FromMinutes(TesterUserTimeZoneOffsetInMinutes); // 180 -> +03:00 (MSK)

        public int TesterUserTimeZoneOffsetInMinutes { get; set; } = 180;
    }
}