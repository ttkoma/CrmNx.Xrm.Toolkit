namespace CrmNx.Xrm.Toolkit.Messages
{
    public class GetAllTimeZonesWithDisplayNameRequest : IWebApiFunction
    {
        private const string ActionUrl =
            "timezonedefinitions/Microsoft.Dynamics.CRM.GetAllTimeZonesWithDisplayName(LocaleId=@LocaleId)";

        public int LocaleId { get; set; }

        /// <summary>
        /// Get currently actived TimeZones with localized display name
        /// </summary>
        /// <param name="localeId">Crm Localization code</param>
        public GetAllTimeZonesWithDisplayNameRequest(int localeId)
        {
            LocaleId = localeId;
        }

        public string QueryString() => $"{ActionUrl}?@LocaleId={LocaleId}";
    }
}