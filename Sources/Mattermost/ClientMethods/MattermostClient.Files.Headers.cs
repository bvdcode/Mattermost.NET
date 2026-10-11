using Mattermost.Constants;
using Mattermost.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<FileHeaders> GetFileHeadersAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetFileHeadersCoreAsync(Routes.Files + "/" + id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<FileHeaders> GetFilePreviewHeadersAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetFileHeadersCoreAsync(Routes.Files + "/" + id + "/preview", cancellationToken);
        }

        /// <inheritdoc />
        public Task<FileHeaders> GetFileThumbnailHeadersAsync(string fileId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(fileId, nameof(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            return GetFileHeadersCoreAsync(Routes.Files + "/" + id + "/thumbnail", cancellationToken);
        }

        private async Task<FileHeaders> GetFileHeadersCoreAsync(string route, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Head, route,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw CreateRequestException(response, json, HttpMethod.Head, route);
            }
            return new FileHeaders
            {
                ContentLength = response.Content.Headers.ContentLength,
                ContentType = response.Content.Headers.ContentType?.ToString(),
                ContentDisposition = response.Content.Headers.ContentDisposition?.ToString(),
                LastModified = response.Content.Headers.LastModified,
                ETag = response.Headers.ETag?.ToString()
            };
        }
    }
}
