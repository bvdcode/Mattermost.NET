using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SetUserImageAsync(string userId, byte[] image, string fileName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            return SetImageAsync(Routes.Users + "/" + id + "/image", image, fileName, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteUserImageAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.Users + "/" + id + "/image", cancellationToken: cancellationToken);
        }
    }
}
