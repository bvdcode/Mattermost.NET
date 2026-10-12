using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task MarkMarketplaceVisitedByAdminAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.CorePlugins + "/marketplace/first_admin_visit", cancellationToken: cancellationToken);
        }
    }
}
