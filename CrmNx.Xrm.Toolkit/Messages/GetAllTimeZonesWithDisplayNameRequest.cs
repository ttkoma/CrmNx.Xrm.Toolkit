namespace CrmNx.Xrm.Toolkit.Messages
{
    public class GetAllTimeZonesWithDisplayNameRequest : OrganizationRequest<GetAllTimeZonesWithDisplayNameResponse>
    {
        private const string WebApiFunctionName = "Microsoft.Dynamics.CRM.GetAllTimeZonesWithDisplayName";

        /// <summary>
        ///     Get currently active TimeZones with localized display name
        /// </summary>
        /// <param name="localeId">Crm Localization code</param>
        public GetAllTimeZonesWithDisplayNameRequest(int localeId) : base(WebApiFunctionName, false)
        {
            LocaleId = localeId;
        }

        public override string RequestBindingPath => "timezonedefinitions";

        public int LocaleId
        {
            get => Parameters.ContainsKey(nameof(LocaleId)) ? (int)Parameters[nameof(LocaleId)] : -1;
            set => Parameters[nameof(LocaleId)] = value;
        }

        //public string QueryString() => $"{ActionUrl}?@LocaleId={LocaleId}";
    }
}