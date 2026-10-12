using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task ReloadServerConfigAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.Config + "/reload", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task InvalidateServerCachesAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.Caches + "/invalidate", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task RecycleDatabaseConnectionsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.Database + "/recycle", cancellationToken: cancellationToken);
        }
    }
}
