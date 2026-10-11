using Mattermost.Constants;
using Mattermost.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<UploadSession> CreateUploadAsync(string channelId, string fileName, long fileSize,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            EscapeReadIdentifier(channelId, nameof(channelId));
            if (string.IsNullOrWhiteSpace(fileName)) { throw new ArgumentException("File name cannot be null or empty.", nameof(fileName)); }
            if (fileSize <= 0) { throw new ArgumentOutOfRangeException(nameof(fileSize), "File size must be greater than zero."); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { channel_id = channelId.Trim(), filename = fileName.Trim(), file_size = fileSize };
            return SendRequestAsync<UploadSession>(HttpMethod.Post, Routes.Uploads, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<UploadSession> GetUploadAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(uploadId, nameof(uploadId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UploadSession>(HttpMethod.Get, Routes.Uploads + "/" + id, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FileDetails?> UploadDataAsync(string uploadId, byte[] data, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(uploadId, nameof(uploadId));
            if (data is null) { throw new ArgumentNullException(nameof(data)); }
            if (data.Length == 0) { throw new ArgumentException("Upload data cannot be empty.", nameof(data)); }
            cancellationToken.ThrowIfCancellationRequested();
            byte[] snapshot = (byte[])data.Clone();
            using ByteArrayContent content = new ByteArrayContent(snapshot);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            string url = Routes.Uploads + "/" + id;
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Post, url, content: content,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NoContent) { return null; }
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) { throw CreateRequestException(response, json, HttpMethod.Post, url); }
            return JsonSerializer.Deserialize<FileDetails>(json) ?? throw new JsonException("The completed upload has no file metadata.");
        }
    }
}
