namespace CrmNx.Xrm.Toolkit.Messages
{
    public class SearchRequest : OrganizationRequest<SearchResponse>
    {
        private const string WebApiFunctionName = "Search";

        public SearchRequest() : base(WebApiFunctionName, false)
        {
        }

        //private const string QueryBase = "Search(AppointmentRequest=@p1)";

        public AppointmentRequest AppointmentRequest
        {
            get => Parameters.ContainsKey(nameof(AppointmentRequest))
                ? (AppointmentRequest) Parameters[nameof(AppointmentRequest)]
                : default;
            set => Parameters[nameof(AppointmentRequest)] = value;
        }

        //public string QueryString()
        //{
        //    var requestJson = JsonConvert.SerializeObject(AppointmentRequest);

        //    return $"{QueryBase}?@p1={requestJson}";
        //}
    }
}