using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<byte[]> GetTeamImageAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.Teams + "/" + id + "/image", cancellationToken);
        }

        /// <inheritdoc />
        public Task SetTeamImageAsync(string teamId, byte[] image, string fileName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            return SetImageAsync(Routes.Teams + "/" + id + "/image", image, fileName, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteTeamImageAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.Teams + "/" + id + "/image", cancellationToken: cancellationToken);
        }
    }
}
