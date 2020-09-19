using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class SearchRequest : IWebApiFunction
    {
        private const string QueryBase = "Search(AppointmentRequest=@p1)";

        public AppointmentRequest AppointmentRequest { get; set; }

        public string QueryString()
        {
            var requestJson = JsonConvert.SerializeObject(AppointmentRequest);

            return $"{QueryBase}?@p1={requestJson}";
        }
    }
}