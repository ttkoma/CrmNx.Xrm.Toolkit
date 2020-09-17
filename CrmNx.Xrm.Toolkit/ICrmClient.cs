using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;

namespace CrmNx.Xrm.Toolkit
{
    public interface ICrmClient
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

        /// <summary>
        /// Create entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        Task<Guid> CreateAsync(Entity entity);

        Task UpdateAsync(Entity entity);

        Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        Task<Entity> RetrieveAsync(EntityReference entityReference, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml,
            CancellationToken cancellationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(string entityName, [AllowNull] QueryOptions options = null,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string entityName, Guid id);

        Task<TResponse> ExecuteFunctionAsync<TResponse>(string query, CancellationToken cancellationToken = default);

        Task<TResponse> ExecuteActionAsync<TResponse>(string query, object parameters,
            CancellationToken cancellationToken = default);

        Task<TResponse> ExecuteAsync<TResponse>(IWebApiFunction apiFunctionRequest,
            CancellationToken cancellationToken = default);

        Task<TResponse> ExecuteAsync<TResponse>(IWebApiAction apiActionRequest,
            CancellationToken cancellationToken = default);
    }
}