using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Groups;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Group>> GetGroupsAsync(int page = 0, int perPage = 60, GroupSearchOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Groups + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            if (options is GroupSearchOptions filters)
            {
                if (filters.Since < 0) { throw new ArgumentOutOfRangeException(nameof(options), "Since cannot be negative."); }
                route += "&include_member_count=" + filters.IncludeMemberCount.ToString().ToLowerInvariant()
                    + "&filter_allow_reference=" + filters.FilterAllowReference.ToString().ToLowerInvariant()
                    + "&include_archived=" + filters.IncludeArchived.ToString().ToLowerInvariant()
                    + "&since=" + filters.Since.ToString(CultureInfo.InvariantCulture);
                if (filters.Query is string query) { route += "&q=" + Uri.EscapeDataString(query.Trim()); }
                if (filters.NotAssociatedToTeam is string team) { route += "&not_associated_to_team=" + EscapeReadIdentifier(team, nameof(options)); }
                if (filters.NotAssociatedToChannel is string channel) { route += "&not_associated_to_channel=" + EscapeReadIdentifier(channel, nameof(options)); }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Group>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Group> GetGroupAsync(string groupId, bool includeMemberCount = false, bool includeMemberIds = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "?include_member_count=" + includeMemberCount.ToString().ToLowerInvariant()
                + "&include_member_ids=" + includeMemberIds.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Group>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Group>> GetGroupsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            List<string> body = PrepareGroupStringList(names, nameof(names), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Group>>(HttpMethod.Post, Routes.Groups + "/names", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Group> CreateGroupAsync(string name, string displayName, IEnumerable<string>? userIds = null,
            string description = "", CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("A mention name is required.", nameof(name)); }
            if (string.IsNullOrWhiteSpace(displayName)) { throw new ArgumentException("A display name is required.", nameof(displayName)); }
            if (description is null) { throw new ArgumentNullException(nameof(description)); }
            List<string> users = new List<string>();
            if (userIds is object) { users = PrepareGroupStringList(userIds, nameof(userIds), cancellationToken); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                name = name.Trim(), display_name = displayName.Trim(), description = description.Trim(),
                source = "custom", allow_reference = true, user_ids = users
            };
            return SendRequestAsync<Group>(HttpMethod.Post, Routes.Groups, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Group> PatchGroupAsync(string groupId, string? name = null, string? displayName = null,
            string? description = null, bool? allowReference = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/patch";
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "name", name?.Trim());
            AddPatchValue(body, "display_name", displayName?.Trim());
            AddPatchValue(body, "description", description?.Trim());
            AddPatchValue(body, "allow_reference", allowReference);
            if (body.Count == 0) { throw new ArgumentException("At least one group field must be supplied."); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Group>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Group> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Group>(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Group> RestoreGroupAsync(string groupId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/restore";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Group>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        private static string GetGroupRoute(string groupId) => Routes.Groups + "/" + EscapeReadIdentifier(groupId, nameof(groupId));

        private static List<string> PrepareGroupStringList(IEnumerable<string> values, string parameterName, CancellationToken cancellationToken)
        {
            if (values is null) { throw new ArgumentNullException(parameterName); }
            List<string> result = new List<string>();
            foreach (string value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("Values cannot be blank.", parameterName); }
                result.Add(value.Trim());
            }
            return result;
        }
    }
}
