using Mattermost.Constants;
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
        public async Task<long> GetFilteredUserCountAsync(string? teamId = null, string? channelId = null, bool includeDeleted = false,
            bool includeBots = false, IEnumerable<string>? roles = null, IEnumerable<string>? teamRoles = null,
            IEnumerable<string>? channelRoles = null, bool includeRemoteUsers = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (teamRoles is object && teamId is null) { throw new ArgumentException("A team filter is required for team roles.", nameof(teamId)); }
            if (channelRoles is object && channelId is null) { throw new ArgumentException("A channel filter is required for channel roles.", nameof(channelId)); }
            cancellationToken.ThrowIfCancellationRequested();
            string query = "include_deleted=" + includeDeleted.ToString().ToLowerInvariant()
                + "&include_bots=" + includeBots.ToString().ToLowerInvariant()
                + "&include_remote_users=" + includeRemoteUsers.ToString().ToLowerInvariant();
            if (teamId is string team) { query += "&in_team=" + EscapeReadIdentifier(team, nameof(teamId)); }
            if (channelId is string channel) { query += "&in_channel=" + EscapeReadIdentifier(channel, nameof(channelId)); }
            query = AppendUserCountRoles(query, "roles", roles, nameof(roles), cancellationToken);
            query = AppendUserCountRoles(query, "team_roles", teamRoles, nameof(teamRoles), cancellationToken);
            query = AppendUserCountRoles(query, "channel_roles", channelRoles, nameof(channelRoles), cancellationToken);
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.Users + "/stats/filtered?" + query,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("total_users_count", out JsonElement count)
                || count.ValueKind != JsonValueKind.Number || !count.TryGetInt64(out long total))
            {
                throw new JsonException("Filtered user count is missing or invalid.");
            }
            return total;
        }

        private static string AppendUserCountRoles(string query, string queryName, IEnumerable<string>? roleNames,
            string parameterName, CancellationToken cancellationToken)
        {
            if (roleNames is null) { return query; }
            List<string> names = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string role in roleNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(role) || role.IndexOf(',') >= 0)
                {
                    throw new ArgumentException("Role names must be nonblank and cannot contain commas.", parameterName);
                }
                string name = role.Trim();
                if (seen.Add(name)) { names.Add(name); }
            }
            if (names.Count == 0) { throw new ArgumentException("At least one role name is required.", parameterName); }
            return query + "&" + queryName + "=" + Uri.EscapeDataString(string.Join(",", names));
        }
    }
}
