using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public static class ICrmWebApiClientExtensions
    {
        public static async Task<ICollection<TimezoneDefinition>> GetAllTimeZonesWithDisplayNameAsync(
            this ICrmWebApiClient apiClient, int localeId,
            CancellationToken cancellationToken)
        {
            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            var request = new GetAllTimeZonesWithDisplayNameRequest(localeId);
            var response = await apiClient.ExecuteAsync<GetAllTimeZonesWithDisplayNameResponse>(request, cancellationToken)
                .ConfigureAwait(false);

            return response.Items.ToArray();
        }
        
        public static async Task<SearchResults> SearchAsync(this ICrmWebApiClient apiClient,
            AppointmentRequest appointmentRequest, CancellationToken cancellationToken)
        {
            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            if (appointmentRequest == null)
                throw new ArgumentNullException(nameof(appointmentRequest));

            var request = new SearchRequest() {AppointmentRequest = appointmentRequest};

            var response = await apiClient.ExecuteAsync<SearchResponse>(request, cancellationToken)
                .ConfigureAwait(false);

            return response.SearchResults;
        }
    }
}