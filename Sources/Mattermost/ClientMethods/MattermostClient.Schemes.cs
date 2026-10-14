using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Helpers;
using Mattermost.Models.Channels;
using Mattermost.Models.Schemes;
using Mattermost.Models.Teams;
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
        public Task<IList<Scheme>> GetSchemesAsync(int page = 0, int perPage = 60, SchemeScope? scope = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Schemes + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            if (scope is SchemeScope value) { route += "&scope=" + GetSchemeScopeName(value); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Scheme>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Scheme> GetSchemeAsync(string schemeId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSchemeRoute(schemeId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Scheme>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Scheme> CreateSchemeAsync(string displayName, SchemeScope scope, string? name = null,
            string description = "", CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(displayName)) { throw new ArgumentException("A display name is required.", nameof(displayName)); }
            if (description is null) { throw new ArgumentNullException(nameof(description)); }
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["display_name"] = displayName.Trim(), ["scope"] = GetSchemeScopeName(scope), ["description"] = description.Trim()
            };
            AddPatchValue(body, "name", name?.Trim());
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Scheme>(HttpMethod.Post, Routes.Schemes, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Scheme> PatchSchemeAsync(string schemeId, string? name = null, string? displayName = null,
            string? description = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSchemeRoute(schemeId) + "/patch";
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "name", name?.Trim());
            AddPatchValue(body, "display_name", displayName?.Trim());
            AddPatchValue(body, "description", description?.Trim());
            if (body.Count == 0) { throw new ArgumentException("At least one scheme field must be supplied."); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Scheme>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteSchemeAsync(string schemeId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSchemeRoute(schemeId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Team>> GetSchemeTeamsAsync(string schemeId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSchemeRoute(schemeId) + "/teams?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Team>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetSchemeChannelsAsync(string schemeId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSchemeRoute(schemeId) + "/channels?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        private static string GetSchemeRoute(string schemeId) => Routes.Schemes + "/" + EscapeReadIdentifier(schemeId, nameof(schemeId));

        private static string GetSchemeScopeName(SchemeScope scope)
        {
            switch (scope)
            {
                case SchemeScope.Team: return "team";
                case SchemeScope.Channel: return "channel";
                default: throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }
    }
}
