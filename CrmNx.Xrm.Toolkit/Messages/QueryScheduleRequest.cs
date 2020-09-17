using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class QueryScheduleRequest : IWebApiFunction
    {
        public Guid ResourceId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public IEnumerable<TimeCode> TimeCodes { get; set; } = Array.Empty<TimeCode>();

        private const string QueryBase =
            "QuerySchedule(ResourceId=@ResourceId,Start=@Start,End=@End,TimeCodes=@TimeCodes)";

        public string QueryString()
        {
            var timeCodesStringArray = TimeCodes.Select(x => x.ToString("d")).ToArray();

            var parameters = new Dictionary<string, string>()
            {
                {"@ResourceId", $"{ResourceId}"},
                {"@Start", $"{Start.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"},
                {"@End", $"{End.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"},
                {"@TimeCodes", $"{JsonConvert.SerializeObject(timeCodesStringArray)}"}
            };

            return QueryHelpers.AddQueryString(QueryBase, parameters);
        }
    }
}