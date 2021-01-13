using System;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Specifies a set of time blocks with appointment information.
    /// </summary>
    public class TimeInfo
    {
        /// <summary>
        ///     The start time for the block.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     The end time for the block.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     Indicates whether the time block is available, busy, filtered or unavailable.
        /// </summary>
        public TimeCode TimeCode { get; set; }

        /// <summary>
        ///     Information about the time block such as whether it is an appointment, break, or holiday.
        /// </summary>
        public SubCode SubCode { get; set; }

        /// <summary>
        ///     The ID of the record referred to in the time block.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        ///     The ID of the calendar for this block of time.
        /// </summary>
        public Guid? CalendarId { get; set; }

        /// <summary>
        ///     The type of entity referred to in the time block.
        /// </summary>
        public int SourceTypeCode { get; set; }

        /// <summary>
        ///     Indicates whether the block of time refers to an activity.
        /// </summary>
        public bool IsActivity { get; set; }

        /// <summary>
        ///     The status of the activity.
        /// </summary>
        public int? ActivityStatusCode { get; set; }

        /// <summary>
        ///     The amount of effort required for this block of time.
        /// </summary>
        public double Effort { get; set; }

        /// <summary>
        ///     The display text shown in the calendar for the time block.
        /// </summary>
        public string DisplayText { get; set; }
    }
}