using Mattermost.Constants;
using Mattermost.Models.Posts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Draft>> GetDraftsAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/me/teams/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/drafts";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Draft>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<Draft>());
        }

        /// <inheritdoc />
        public Task<Draft?> UpsertDraftAsync(Draft draft, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (draft is null) { throw new ArgumentNullException(nameof(draft)); }
            if (string.IsNullOrWhiteSpace(draft.ChannelId)) { throw new ArgumentException("A draft must have a channel identifier.", nameof(draft)); }
            if (draft.Text is null) { throw new ArgumentException("Draft text cannot be null. Use an empty string to delete the draft.", nameof(draft)); }
            cancellationToken.ThrowIfCancellationRequested();
            List<string> files = new List<string>();
            if (draft.FileIdentifiers is IList<string> identifiers)
            {
                foreach (string id in identifiers)
                {
                    if (string.IsNullOrWhiteSpace(id)) { throw new ArgumentException("File identifiers cannot be blank.", nameof(draft)); }
                    files.Add(id.Trim());
                }
            }
            var body = new
            {
                channel_id = draft.ChannelId.Trim(), root_id = draft.RootId?.Trim() ?? string.Empty,
                message = draft.Text, type = draft.Type,
                props = draft.Props is IDictionary<string, object> props ? new Dictionary<string, object>(props) : null,
                file_ids = files,
                priority = draft.Priority is IDictionary<string, object> priority ? new Dictionary<string, object>(priority) : null
            };
            return SendRequestAsync<Draft?>(HttpMethod.Post, Routes.Drafts, body, cancellationToken, nullResultFactory: () => null);
        }

        /// <inheritdoc />
        public Task DeleteDraftAsync(string channelId, string? threadId = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/me/channels/" + EscapeReadIdentifier(channelId, nameof(channelId)) + "/drafts";
            if (threadId is string id) { route += "/" + EscapeReadIdentifier(id, nameof(threadId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }
    }
}
