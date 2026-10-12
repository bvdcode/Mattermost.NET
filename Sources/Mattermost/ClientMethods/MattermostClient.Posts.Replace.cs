using Mattermost.Constants;
using Mattermost.Models.Posts;
using System;
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
        public Task<Post> ReplacePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (post is null) { throw new ArgumentNullException(nameof(post)); }
            string identifier = EscapeReadIdentifier(post.Id, nameof(post));
            if (post.Text is null) { throw new ArgumentException("Post text cannot be null.", nameof(post)); }
            if (post.Text.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(post), "Post text exceeds the maximum supported length.");
            }
            cancellationToken.ThrowIfCancellationRequested();

            List<string>? files = null;
            if (post.FileIdentifiers is object)
            {
                files = new List<string>();
                foreach (string file in post.FileIdentifiers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(file)) { throw new ArgumentException("File identifiers cannot be blank.", nameof(post)); }
                    files.Add(file.Trim());
                }
            }
            JsonElement? props = post.RawProps is null ? (JsonElement?)null : JsonSerializer.SerializeToElement(post.RawProps);
            var body = new
            {
                id = post.Id.Trim(),
                message = post.Text,
                is_pinned = post.IsPinned,
                has_reactions = post.HasReactions,
                props,
                file_ids = files
            };
            return SendRequestAsync<Post>(HttpMethod.Put, Routes.Posts + "/" + identifier, body, cancellationToken);
        }
    }
}
