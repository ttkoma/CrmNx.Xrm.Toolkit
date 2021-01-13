using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Provides the details of an appointment request for the Search function.
    /// </summary>
    public class AppointmentRequest
    {
        /// <summary>
        ///     The ID of the service to search for.
        /// </summary>
        public Guid ServiceId { get; set; }

        /// <summary>
        ///     The time offset in minutes, from midnight, when the first occurrence of the appointment can take place.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? AnchorOffset { get; set; }

        /// <summary>
        ///     The time zone code of the user who is requesting the appointment.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? UserTimeZoneCode { get; set; }

        /// <summary>
        ///     The time, in minutes, for which the appointment recurrence is valid.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RecurrenceDuration { get; set; }

        /// <summary>
        ///     A value to override the time zone that is specified by the UserTimeZoneCode property.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RecurrenceTimeZoneCode { get; set; }

        /// <summary>
        ///     The appointments to ignore in the search for possible appointments.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<AppointmentsToIgnore> AppointmentsToIgnore { get; set; }

        /// <summary>
        ///     The resources that are needed for this appointment.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RequiredResource> RequiredResources { get; set; }

        /// <summary>
        ///     The date and time to begin the search.
        /// </summary>
        public DateTime SearchWindowStart { get; set; }

        /// <summary>
        ///     The date and time to end the search.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? SearchWindowEnd { get; set; }

        /// <summary>
        ///     The date and time for the first possible instance of the appointment.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? SearchRecurrenceStart { get; set; }

        /// <summary>
        ///     The recurrence rule for appointment recurrence.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SearchRecurrenceRule { get; set; }

        /// <summary>
        ///     The appointment duration, in minutes.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Duration { get; set; }

        /// <summary>
        ///     Any additional constraints.
        /// </summary>
        /// <returns></returns>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ConstraintRelation> Constraints { get; set; }

        /// <summary>
        ///     The scheduling strategy that overrides the default constraints.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ObjectiveRelation> Objectives { get; set; }

        /// <summary>
        ///     The direction of the search.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SearchDirection? Direction { get; set; }

        /// <summary>
        ///     The number of results to be returned from the search request.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int NumberOfResults { get; set; } = 1;

        /// <summary>
        ///     The sites where the requested appointment can take place.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Guid> Sites { get; set; }
    }

    /// <summary>
    ///     Contains the data that describes the scheduling strategy for an AppointmentRequest and that overrides the default
    ///     constraints.
    /// </summary>
    public class ObjectiveRelation
    {
        /// <summary>
        ///     The ID of the resource specification.
        /// </summary>
        public Guid ResourceSpecId { get; set; }

        /// <summary>
        ///     The search strategy to use in the appointment request for the SearchRequest message.
        /// </summary>
        public string ObjectiveExpression { get; set; }
    }

    /// <summary>
    ///     Specifies additional constraints to be applied when you select resources for appointments.
    /// </summary>
    public class ConstraintRelation
    {
        /// <summary>
        ///     The ID of the calendar rule to which the constraint is applied.
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        ///     The type of constraints.
        /// </summary>
        public string ConstraintType { get; set; }

        /// <summary>
        ///     The set of additional constraints.
        /// </summary>
        public string Constraints { get; set; }
    }

    /// <summary>
    ///     Specifies a resource that is required for a scheduling operation.
    /// </summary>
    public class RequiredResource
    {
        /// <summary>
        ///     The ID of the required resource.
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        ///     The ID of the required resource specification
        /// </summary>
        public Guid ResourceSpecId { get; set; }
    }

    /// <summary>
    ///     Specifies the appointments to ignore in an appointment request from the Search function.
    /// </summary>
    public class AppointmentsToIgnore
    {
        /// <summary>
        ///     An array of IDs of appointments to ignore.
        /// </summary>
        public ICollection<Guid> Appointments { get; set; }

        /// <summary>
        ///     The resource for which appointments are to be ignored.
        /// </summary>
        public Guid ResourceId { get; set; }
    }

    /// <summary>
    ///     Contains the possible values for the search direction in an appointment request.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), false)]
    public enum SearchDirection
    {
        /// <summary>
        ///     Search forward in the calendar.
        /// </summary>
        Forward = 0,

        /// <summary>
        ///     Search backward in the calendar.
        /// </summary>
        Backward = 1
    }
}