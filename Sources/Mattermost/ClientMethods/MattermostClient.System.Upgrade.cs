using Mattermost.Constants;
using Mattermost.Models.Responses;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task CheckEnterpriseUpgradeAllowedAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Get, Routes.EnterpriseUpgrade + "/allowed", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<EnterpriseUpgradeStatus> GetEnterpriseUpgradeStatusAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<EnterpriseUpgradeStatus>(HttpMethod.Get, Routes.EnterpriseUpgrade + "/status", cancellationToken: cancellationToken);
        }
    }
}
