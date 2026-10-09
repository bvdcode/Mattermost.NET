using Mattermost.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<byte[]> GetUserImageAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.Users + "/" + id + "/image", cancellationToken);
        }

        /// <inheritdoc />
        public Task<byte[]> GetDefaultUserImageAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.Users + "/" + id + "/image/default", cancellationToken);
        }
    }
}
