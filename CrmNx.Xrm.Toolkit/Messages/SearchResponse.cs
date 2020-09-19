using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    /// Contains the response from the Search function.
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// The results of the search.
        /// </summary>
        /// <returns></returns>
        public SearchResults SearchResults { get; set; }
    }

    public class SearchResults
    {
        /// <summary>
        /// The set of proposed appointments that meet the appointment request criteria.
        /// </summary>
        public ICollection<AppointmentProposal> Proposals { get; set; }

        /// <summary>
        /// Information regarding the results of the search.
        /// </summary>
        public TraceInfo TraceInfo { get; set; }
    }

    public class TraceInfo
    {
        public ICollection<ErrorInfo> ErrorInfoList { get; set; }
    }

    /// <summary>
    /// Specifies the results of a scheduling operation using the ValidateRequest, BookRequest, or Reschedule action.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// The array of information about a resource that has a scheduling problem for an appointment.
        /// </summary>
        public ICollection<ResourceInfo> ResourceList { get; set; }

        /// <summary>
        /// The reason for a scheduling failure.
        /// </summary>
        public string ErrorCode { get; set; }
    }

    /// <summary>
    /// Contains information about a resource that has a scheduling problem for an appointment.
    /// </summary>
    public class ResourceInfo
    {
        /// <summary>
        /// The ID of the record that has a scheduling problem.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name for the resource.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The logical name of the entity.
        /// </summary>
        public string EntityName { get; set; }
    }

    /// <summary>
    /// Represents a proposed appointment time and date as a result of the Search function.
    /// </summary>
    public class AppointmentProposal
    {
        /// <summary>
        /// The proposed appointment start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// The proposed appointment end date and time.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// The ID of the site for the proposed appointment.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// The name of the site for the proposed appointment.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// An array of parties needed for the proposed appointment.
        /// </summary>
        public ICollection<ProposalParty> ProposalParties { get; set; }
    }

    public class ProposalParty
    {
        /// <summary>
        /// The ID of the resource that is represented by this party.
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The ID of the resource specification that is represented by this party.
        /// </summary>
        public Guid ResourceSpecId { get; set; }

        /// <summary>
        /// The display name for the party.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The logical name of the type of entity that is represented by this party.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The percentage of time that is required to perform the service.
        /// </summary>
        public double EffortRequired { get; set; }
    }
}