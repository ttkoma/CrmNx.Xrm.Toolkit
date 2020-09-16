using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Toolkit
{
    public interface ICrmClient
    {
        /// <summary>
        /// Gets or sets the current caller Id.
        /// </summary>
        Guid CallerId { get; set; }

        /// <summary>
        /// Returns the user Id of the currently logged in user.
        /// </summary>
        /// <returns></returns>
        Guid GetMyCrmUserId();

        Task<Guid> CreateAsync(Entity entity);

        Task<Entity> RetrieveAsync(string entityName, Guid id, [AllowNull] QueryOptions options = null, CancellationToken cancellationToken = default);

        Task<Entity> RetrieveAsync(EntityReference entityReference, [AllowNull] QueryOptions options = null, CancellationToken cancellationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(FetchXmlExpression fetchXml, CancellationToken cancelationToken = default);

        Task<EntityCollection> RetrieveMultipleAsync(string entityName, [AllowNull] QueryOptions options = null, CancellationToken cancelationToken = default);

        Task UpdateAsync(Entity entity);

        Task DeleteAsync(string entityName, Guid id);

        Task<T> ExecuteFunctionAsync<T>(string query, CancellationToken cancelationToken = default);

        Task<TResponse> ExecuteFunctionAsync<TResponse>(IWebApiFunction request, CancellationToken cancelationToken = default);
    }
}
