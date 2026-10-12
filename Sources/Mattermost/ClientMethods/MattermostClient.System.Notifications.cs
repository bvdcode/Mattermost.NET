using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SendTestNotificationAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.NotificationTest, cancellationToken: cancellationToken);
        }
    }
}
