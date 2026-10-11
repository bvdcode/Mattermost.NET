using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Channels;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<string>> GetKnownUserIdsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, Routes.Users + "/known",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<string>());
        }

        /// <inheritdoc />
        public Task<UserAutocomplete> AutocompleteUsersAsync(string name, string? teamId = null, string? channelId = null,
            int limit = 100, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            if (limit <= 0) { throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero."); }
            if (teamId is object) { ValidateUserReadIdentifier(teamId, nameof(teamId)); }
            if (channelId is object)
            {
                ValidateUserReadIdentifier(channelId, nameof(channelId));
                if (teamId is null) { throw new ArgumentException("A team identifier is required when filtering by channel.", nameof(teamId)); }
            }
            cancellationToken.ThrowIfCancellationRequested();
            string query = "name=" + Uri.EscapeDataString(name.Trim()) + "&limit=" + limit.ToString(CultureInfo.InvariantCulture);
            if (teamId is object) { query += "&in_team=" + Uri.EscapeDataString(teamId.Trim()); }
            if (channelId is object) { query += "&in_channel=" + Uri.EscapeDataString(channelId.Trim()); }
            return SendRequestAsync<UserAutocomplete>(HttpMethod.Get, Routes.Users + "/autocomplete?" + query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> GetTotalUserCountAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.Users + "/stats",
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("total_users_count", out JsonElement count)
                || count.ValueKind != JsonValueKind.Number || !count.TryGetInt64(out long total))
            {
                throw new JsonException("Total user count is missing or invalid.");
            }
            return total;
        }

        /// <inheritdoc />
        public Task<IDictionary<string, IList<User>>> GetUsersByGroupChannelIdsAsync(IEnumerable<string> channelIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareStringBatch(channelIds, nameof(channelIds));
            if (ids.Count > 50) { throw new ArgumentException("At most 50 group-channel identifiers can be requested at once.", nameof(channelIds)); }
            return SendRequestAsync<IDictionary<string, IList<User>>>(HttpMethod.Post, Routes.Users + "/group_channels", ids,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<ChannelMemberWithTeamData>> GetUserChannelMembersAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateUserReadIdentifier(userId, nameof(userId));
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<ChannelMemberWithTeamData>>(HttpMethod.Get,
                Routes.Users + "/" + Uri.EscapeDataString(userId.Trim()) + "/channel_members?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<ChannelMemberWithTeamData>());
        }

        /// <inheritdoc />
        public Task<IList<UserSession>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateUserReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UserSession>>(HttpMethod.Get,
                Routes.Users + "/" + Uri.EscapeDataString(userId.Trim()) + "/sessions",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<UserSession>());
        }

        /// <inheritdoc />
        public Task<IList<UserAudit>> GetUserAuditsAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateUserReadIdentifier(userId, nameof(userId));
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UserAudit>>(HttpMethod.Get,
                Routes.Users + "/" + Uri.EscapeDataString(userId.Trim()) + "/audits?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<UserAudit>());
        }

        private static void ValidateUserReadIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("The identifier cannot be null or empty.", parameterName); }
        }
    }
}
