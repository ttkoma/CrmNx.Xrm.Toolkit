using System.Runtime.Serialization;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    /// Contains the possible values for a subcode, used in scheduling appointments.
    /// </summary>
    public enum SubCode
    {
        /// <summary>
        /// Specifies free time with no specified restrictions.
        /// </summary>
        [EnumMember]
        Unspecified = 0,

        /// <summary>
        /// A schedulable block of time.
        /// </summary>
        [EnumMember]
        Schedulable = 1,

        /// <summary>
        /// A block of time that is committed to perform an action.
        /// </summary>
        [EnumMember]
        Committed = 2,

        /// <summary>
        /// A block of time that is tentatively scheduled but not committed.
        /// </summary>
        [EnumMember]
        Uncommitted = 3,

        /// <summary>
        /// A block of time that cannot be committed due to a scheduled break.
        /// </summary>
        [EnumMember]
        Break = 4,

        /// <summary>
        /// A block of time that cannot be scheduled due to a scheduled holiday.
        /// </summary>
        [EnumMember]
        Holiday = 5,

        /// <summary>
        /// A block of time that cannot be scheduled due to a scheduled vacation.
        /// </summary>
        [EnumMember]
        Vacation = 6,

        /// <summary>
        /// A block of time that is already scheduled for an appointment.
        /// </summary>
        [EnumMember]
        Appointment = 7,

        /// <summary>
        /// Specifies to filter a resource start time.
        /// </summary>
        [EnumMember]
        ResourceStartTime = 8,

        /// <summary>
        /// A restriction for a resource for the specified service.
        /// </summary>
        [EnumMember]
        ResourceServiceRestriction = 9,

        /// <summary>
        /// Specifies the capacity of a resource for the specified time interval.
        /// </summary>
        [EnumMember]
        ResourceCapacity = 10,

        /// <summary>
        /// Specifies that a service is restricted during the specified block of time.
        /// </summary>
        [EnumMember]
        ServiceRestriction = 11,

        /// <summary>
        /// An override to the service cost for the specified time block.
        /// </summary>
        [EnumMember]
        ServiceCost = 12
    }
}
