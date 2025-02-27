using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.ObjectModel;

namespace CrmNx.Xrm.Toolkit
{
    public interface ICrmWebApiClient
    {
        /// <summary>
        /// Gets or sets the current caller Id.
        /// </summary>
        Guid CallerId { get; set; }

        /// <summary>
        /// Metadata Service
        /// </summary>
        IWebApiMetadataService WebApiMetadata { get; }

        /// <summary>
        /// Returns the user Id of the currently logged in crm.
        /// </summary>
        /// <returns></returns>
        Guid GetMyCrmUserId();

        Task AssociateAsync(string entityName, Guid entityId, Relationship relationship,
            EntityReference[] relatedEntities);
        
        /// <summary>
        /// Create entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        Task<Guid> CreateAsync(Entity entity);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="allowUpsert">Allow entity creation if doesn't exists (false by default)</param>
        /// <returns></returns>
        Task UpdateAsync(Entity entity, bool allowUpsert = false);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="target">Target entity reference</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebApiException"></exception>
        Task DeleteAsync(EntityReference target);

        /// <summary>
        /// Clear Lookup field
        /// </summary>
        /// <param name="target">Target entity reference</param>
        /// <param name="propertyName">Lookup property name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebApiException"></exception>
        Task DisassociateAsync(EntityReference target, string propertyName);

        Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        Task<Entity> RetrieveAsync(EntityReference entityReference, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml,
            CancellationToken cancellationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(string entityName, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        // Task<TResponse> ExecuteFunctionAsync<TResponse>(string query, CancellationToken cancellationToken = default);

        // Task<TResponse> ExecuteAsync<TResponse>(IWebApiFunction apiFunctionRequest,
        //     CancellationToken cancellationToken = default);

        // Task<TResponse> ExecuteAsync<TResponse>(WebApiActionBase apiActionRequest, CancellationToken cancellationToken = default);

        // Task ExecuteAsync(WebApiActionBase apiActionRequest, CancellationToken cancellationToken = default);

        Task<TResponse> ExecuteAsync<TResponse>(OrganizationRequest<TResponse> request,
            CancellationToken cancellationToken = default);

        // Task<TResponse> ExecuteActionAsync<TResponse>(string query, object parameters, CancellationToken cancellationToken = default);
    }
}