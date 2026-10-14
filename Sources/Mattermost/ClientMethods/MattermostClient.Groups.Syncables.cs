using Mattermost.Models.Groups;
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
        public Task<GroupSyncable> LinkGroupToTeamAsync(string groupId, string teamId, bool? autoAdd = null, bool? schemeAdmin = null,
            CancellationToken cancellationToken = default) => SendGroupSyncablePatchAsync(HttpMethod.Post, groupId, "teams", teamId, "/link", autoAdd, schemeAdmin, cancellationToken);

        /// <inheritdoc />
        public Task<GroupSyncable> LinkGroupToChannelAsync(string groupId, string channelId, bool? autoAdd = null, bool? schemeAdmin = null,
            CancellationToken cancellationToken = default) => SendGroupSyncablePatchAsync(HttpMethod.Post, groupId, "channels", channelId, "/link", autoAdd, schemeAdmin, cancellationToken);

        /// <inheritdoc />
        public Task UnlinkGroupFromTeamAsync(string groupId, string teamId, CancellationToken cancellationToken = default)
            => UnlinkGroupSyncableAsync(groupId, "teams", teamId, cancellationToken);

        /// <inheritdoc />
        public Task UnlinkGroupFromChannelAsync(string groupId, string channelId, CancellationToken cancellationToken = default)
            => UnlinkGroupSyncableAsync(groupId, "channels", channelId, cancellationToken);

        /// <inheritdoc />
        public Task<GroupSyncable> GetGroupTeamAsync(string groupId, string teamId, CancellationToken cancellationToken = default)
            => GetGroupSyncableAsync(groupId, "teams", teamId, cancellationToken);

        /// <inheritdoc />
        public Task<GroupSyncable> GetGroupChannelAsync(string groupId, string channelId, CancellationToken cancellationToken = default)
            => GetGroupSyncableAsync(groupId, "channels", channelId, cancellationToken);

        /// <inheritdoc />
        public Task<IList<GroupSyncable>> GetGroupTeamsAsync(string groupId, CancellationToken cancellationToken = default)
            => GetGroupSyncablesAsync(groupId, "teams", cancellationToken);

        /// <inheritdoc />
        public Task<IList<GroupSyncable>> GetGroupChannelsAsync(string groupId, CancellationToken cancellationToken = default)
            => GetGroupSyncablesAsync(groupId, "channels", cancellationToken);

        /// <inheritdoc />
        public Task<GroupSyncable> PatchGroupTeamAsync(string groupId, string teamId, bool? autoAdd = null, bool? schemeAdmin = null,
            CancellationToken cancellationToken = default) => SendGroupSyncablePatchAsync(HttpMethod.Put, groupId, "teams", teamId, "/patch", autoAdd, schemeAdmin, cancellationToken);

        /// <inheritdoc />
        public Task<GroupSyncable> PatchGroupChannelAsync(string groupId, string channelId, bool? autoAdd = null, bool? schemeAdmin = null,
            CancellationToken cancellationToken = default) => SendGroupSyncablePatchAsync(HttpMethod.Put, groupId, "channels", channelId, "/patch", autoAdd, schemeAdmin, cancellationToken);

        private Task<GroupSyncable> SendGroupSyncablePatchAsync(HttpMethod method, string groupId, string resourceType, string resourceId,
            string suffix, bool? autoAdd, bool? schemeAdmin, CancellationToken cancellationToken)
        {
            CheckDisposed();
            string route = GetGroupSyncableRoute(groupId, resourceType, resourceId) + suffix;
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "auto_add", autoAdd);
            AddPatchValue(body, "scheme_admin", schemeAdmin);
            if (method == HttpMethod.Put && body.Count == 0) { throw new ArgumentException("At least one link field must be supplied."); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupSyncable>(method, route, body, cancellationToken);
        }

        private Task UnlinkGroupSyncableAsync(string groupId, string resourceType, string resourceId, CancellationToken cancellationToken)
        {
            CheckDisposed();
            string route = GetGroupSyncableRoute(groupId, resourceType, resourceId) + "/link";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        private Task<GroupSyncable> GetGroupSyncableAsync(string groupId, string resourceType, string resourceId, CancellationToken cancellationToken)
        {
            CheckDisposed();
            string route = GetGroupSyncableRoute(groupId, resourceType, resourceId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupSyncable>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        private Task<IList<GroupSyncable>> GetGroupSyncablesAsync(string groupId, string resourceType, CancellationToken cancellationToken)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/" + resourceType;
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<GroupSyncable>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        private static string GetGroupSyncableRoute(string groupId, string resourceType, string resourceId)
            => GetGroupRoute(groupId) + "/" + resourceType + "/" + EscapeReadIdentifier(resourceId, nameof(resourceId));
    }
}
