using System.Runtime.Serialization;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public enum TimeCode
    {
        /// <summary>
        /// The time is available within the working hours of the resource.
        /// </summary>
        [EnumMember]
        Available = 0,

        /// <summary>
        /// The time is committed to an activity.
        /// </summary>
        [EnumMember]
        Busy = 1,

        /// <summary>
        /// The time is unavailable.
        /// </summary>
        [EnumMember]
        Unavailable = 2,

        /// <summary>
        /// Use additional filters for the time block such as service cost or service start time.
        /// </summary>
        Filter = 3
    }
}
