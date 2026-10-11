using Mattermost.Constants;
using Mattermost.Models.Emojis;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public async Task<Emoji> CreateEmojiAsync(string name, byte[] image, string fileName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string emojiName = SanitizeEmojiName(name, nameof(name));
            if (image is null) { throw new ArgumentNullException(nameof(image)); }
            if (image.Length == 0) { throw new ArgumentException("An emoji image cannot be empty.", nameof(image)); }
            if (string.IsNullOrWhiteSpace(fileName)) { throw new ArgumentException("A filename is required.", nameof(fileName)); }
            cancellationToken.ThrowIfCancellationRequested();
            byte[] data = (byte[])image.Clone();
            string filename = fileName.Trim();
            await CheckAuthorizedAsync(cancellationToken).ConfigureAwait(false);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            using ByteArrayContent imageContent = new ByteArrayContent(data);
            using StringContent metadata = new StringContent(JsonSerializer.Serialize(new
            {
                name = emojiName, creator_id = _cachedUserInfo!.Id
            }), Encoding.UTF8, "application/json");
            content.Add(metadata, "emoji");
            content.Add(imageContent, "image", filename);
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Post, Routes.Emojis,
                content: content, cancellationToken: cancellationToken).ConfigureAwait(false);
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) { throw CreateRequestException(response, json, HttpMethod.Post, Routes.Emojis); }
            return JsonSerializer.Deserialize<Emoji>(json) ?? throw new JsonException("Failed to deserialize emoji response: " + json);
        }

        /// <inheritdoc />
        public Task DeleteEmojiAsync(string emojiId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(emojiId, nameof(emojiId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.Emojis + "/" + id, cancellationToken: cancellationToken);
        }
    }
}
