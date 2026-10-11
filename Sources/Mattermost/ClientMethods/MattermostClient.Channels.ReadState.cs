using Mattermost.Constants;
using Mattermost.Models.Responses;
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
        public Task<ChannelViewResponse> ViewChannelAsync(string userId, string channelId, string? previousChannelId = null,
            bool collapsedThreadsSupported = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Channels + "/members/" + EscapeReadIdentifier(userId, nameof(userId)) + "/view";
            if (channelId is null) { throw new ArgumentNullException(nameof(channelId)); }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["channel_id"] = channelId.Trim(), ["collapsed_threads_supported"] = collapsedThreadsSupported
            };
            if (previousChannelId is string previous) { body["prev_channel_id"] = previous.Trim(); }
            return SendRequestAsync<ChannelViewResponse>(HttpMethod.Post, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelViewResponse> MarkChannelsReadAsync(string userId, IEnumerable<string> channelIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Channels + "/members/" + EscapeReadIdentifier(userId, nameof(userId)) + "/mark_read";
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareStringBatch(channelIds, nameof(channelIds));
            return SendRequestAsync<ChannelViewResponse>(HttpMethod.Post, route, ids, cancellationToken);
        }
    }
}
