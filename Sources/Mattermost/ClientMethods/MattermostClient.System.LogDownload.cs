using Mattermost.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<byte[]> DownloadServerLogsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.ServerLogs + "/download", cancellationToken);
        }
    }
}
