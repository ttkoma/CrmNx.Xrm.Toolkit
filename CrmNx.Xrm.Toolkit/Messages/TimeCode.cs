using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrmNx.Xrm.Toolkit.Messages
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeCode
    {
        /// <summary>
        ///     The time is available within the working hours of the resource.
        /// </summary>
        [EnumMember(Value = "0")] 
        Available = 0,

        /// <summary>
        ///     The time is committed to an activity.
        /// </summary>
        [EnumMember(Value = "1")] 
        Busy = 1,

        /// <summary>
        ///     The time is unavailable.
        /// </summary>
        [EnumMember(Value = "2")] 
        Unavailable = 2,

        /// <summary>
        ///     Use additional filters for the time block such as service cost or service start time.
        /// </summary>
        [EnumMember(Value = "3")]
        Filter = 3
    }
}