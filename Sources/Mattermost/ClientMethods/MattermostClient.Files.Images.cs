using Mattermost.Constants;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<byte[]> GetFilePreviewAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.Files + "/" + id + "/preview", cancellationToken);
        }

        /// <inheritdoc />
        public Task<byte[]> GetFileThumbnailAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(Routes.Files + "/" + id + "/thumbnail", cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> GetPublicFileLinkAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> response = await SendRequestAsync<Dictionary<string, string>>(HttpMethod.Get,
                Routes.Files + "/" + id + "/link", cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.TryGetValue("link", out string? link) || string.IsNullOrWhiteSpace(link))
            {
                throw new JsonException("The public file link is missing from the response.");
            }
            return link;
        }
    }
}
