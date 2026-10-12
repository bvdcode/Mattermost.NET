using Mattermost.Constants;
using Mattermost.Models.Responses;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<ChannelViewResponse> MarkAllDirectMessagesReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Channels + "/members/" + EscapeReadIdentifier(userId, nameof(userId)) + "/direct/read";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ChannelViewResponse>(HttpMethod.Put, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelViewResponse> MarkAllTeamChannelsReadAsync(string userId, string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/teams/"
                + EscapeReadIdentifier(teamId, nameof(teamId)) + "/read";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ChannelViewResponse>(HttpMethod.Put, route, cancellationToken: cancellationToken);
        }
    }
}
