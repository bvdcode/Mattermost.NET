using Mattermost.Constants;
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
        public Task UpdateChannelMemberRolesAsync(string channelId, string userId, string roles,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetChannelMemberRoute(channelId, userId) + "/roles";
            if (roles is null) { throw new ArgumentNullException(nameof(roles)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { roles }, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateChannelMemberSchemeRolesAsync(string channelId, string userId, bool schemeUser, bool schemeAdmin,
            bool schemeGuest = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetChannelMemberRoute(channelId, userId) + "/schemeRoles";
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { scheme_user = schemeUser, scheme_admin = schemeAdmin, scheme_guest = schemeGuest };
            return SendRequestAsync(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateChannelMemberNotifyPropsAsync(string channelId, string userId, IDictionary<string, string> notifyProps,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetChannelMemberRoute(channelId, userId) + "/notify_props";
            if (notifyProps is null) { throw new ArgumentNullException(nameof(notifyProps)); }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> body = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> property in notifyProps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(property.Key) || property.Value is null)
                {
                    throw new ArgumentException("Notification properties require nonblank names and non-null string values.", nameof(notifyProps));
                }
                body.Add(property.Key, property.Value);
            }
            return SendRequestAsync(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IDictionary<string, long>> GetChannelsMemberCountsAsync(IEnumerable<string> channelIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareStringBatch(channelIds, nameof(channelIds));
            return SendRequestAsync<IDictionary<string, long>>(HttpMethod.Post, Routes.Channels + "/stats/member_count", ids,
                cancellationToken);
        }

        private static string GetChannelMemberRoute(string channelId, string userId) => Routes.Channels + "/"
            + EscapeReadIdentifier(channelId, nameof(channelId)) + "/members/" + EscapeReadIdentifier(userId, nameof(userId));
    }
}
