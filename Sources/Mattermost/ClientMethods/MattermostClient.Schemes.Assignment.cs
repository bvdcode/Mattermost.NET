using Mattermost.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SetTeamSchemeAsync(string teamId, string schemeId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Teams + "/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/scheme";
            if (schemeId is null) { throw new ArgumentNullException(nameof(schemeId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { scheme_id = schemeId.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task SetChannelSchemeAsync(string channelId, string schemeId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Channels + "/" + EscapeReadIdentifier(channelId, nameof(channelId)) + "/scheme";
            EscapeReadIdentifier(schemeId, nameof(schemeId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { scheme_id = schemeId.Trim() }, cancellationToken);
        }
    }
}
