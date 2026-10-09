using Mattermost.Constants;
using Mattermost.Models.Responses;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<SystemStatusResponse> GetSystemStatusAsync(bool checkBackends = false, bool useRestSemantics = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            string route = Routes.System + "/ping?get_server_status=" + checkBackends.ToString().ToLowerInvariant()
                + "&use_rest_semantics=" + useRestSemantics.ToString().ToLowerInvariant();
            return SendRequestAsync<SystemStatusResponse>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                requiresAuthorization: false);
        }

        /// <inheritdoc />
        public Task<IList<string>> GetSupportedTimezonesAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, Routes.System + "/timezones", cancellationToken: cancellationToken);
        }
    }
}
