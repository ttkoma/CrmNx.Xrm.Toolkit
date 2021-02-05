using System;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class QueryScheduleRequest : OrganizationRequest<QueryScheduleResponse>
    {
        private const string WebApiFunctionName = "QuerySchedule";

        public QueryScheduleRequest() : base(WebApiFunctionName, false)
        {
            TimeCodes = Array.Empty<TimeCode>();
        }

        public Guid ResourceId
        {
            get => Parameters.ContainsKey(nameof(ResourceId)) ? (Guid)Parameters[nameof(ResourceId)] : default;
            set => Parameters[nameof(ResourceId)] = value;
        }

        public DateTime Start
        {
            get => Parameters.ContainsKey(nameof(Start)) ? (DateTime)Parameters[nameof(Start)] : default;
            set => Parameters[nameof(Start)] = value;
        }

        public DateTime End
        {
            get => Parameters.ContainsKey(nameof(End)) ? (DateTime)Parameters[nameof(End)] : default;
            set => Parameters[nameof(End)] = value;
        }

        public TimeCode[] TimeCodes
        {
            get => Parameters.ContainsKey(nameof(TimeCodes)) ? (TimeCode[])Parameters[nameof(TimeCodes)] : default;
            set => Parameters[nameof(TimeCodes)] = value;
        }

        // private const string QueryBase =
        //     "QuerySchedule(ResourceId=@ResourceId,Start=@Start,End=@End,TimeCodes=@TimeCodes)";

        // public string QueryString()
        // {
        //     var timeCodesStringArray = TimeCodes.Select(x => x.ToString("d")).ToArray();
        //
        //     var parameters = new Dictionary<string, string>()
        //     {
        //         {"@ResourceId", $"{ResourceId}"},
        //         {"@Start", $"{Start.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"},
        //         {"@End", $"{End.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"},
        //         {"@TimeCodes", $"{JsonConvert.SerializeObject(timeCodesStringArray)}"}
        //     };
        //
        //     return QueryHelpers.AddQueryString(QueryBase, parameters);
        // }
    }
}