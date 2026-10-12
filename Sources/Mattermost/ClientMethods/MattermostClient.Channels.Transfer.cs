using Mattermost.Constants;
using Mattermost.Models.Channels;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<Channel> MoveChannelAsync(string channelId, string teamId, bool force = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            ValidateTeamIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels + "/" + id + "/move",
                new { team_id = teamId.Trim(), force }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Channel> ConvertGroupMessageToChannelAsync(string channelId, string teamId, string name, string displayName,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            ValidateTeamIdentifier(teamId, nameof(teamId));
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("Channel name cannot be null or empty.", nameof(name)); }
            if (string.IsNullOrWhiteSpace(displayName)) { throw new ArgumentException("Channel display name cannot be null or empty.", nameof(displayName)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { channel_id = channelId.Trim(), team_id = teamId.Trim(), name = name.Trim(), display_name = displayName.Trim() };
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels + "/" + id + "/convert_to_channel", body, cancellationToken);
        }
    }
}
