using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Bots;
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
        public Task<IList<Bot>> GetBotsAsync(int page = 0, int perPage = 60, bool includeDeleted = false, bool onlyOrphaned = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildPagedQuery(page, perPage)
                + "&include_deleted=" + includeDeleted.ToString().ToLowerInvariant()
                + "&only_orphaned=" + onlyOrphaned.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Bot>>(HttpMethod.Get, Routes.Bots + "?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<Bot>());
        }

        /// <inheritdoc />
        public Task<Bot> GetBotAsync(string botUserId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId))
                + "?include_deleted=" + includeDeleted.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Bot> CreateBotAsync(string username, string? displayName = null, string? description = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(username)) { throw new ArgumentException("Bot username cannot be blank.", nameof(username)); }
            Dictionary<string, string> body = PrepareBotMetadata(username, displayName, description);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Post, Routes.Bots, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Bot> PatchBotAsync(string botUserId, string? username = null, string? displayName = null, string? description = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId));
            if (username is null && displayName is null && description is null)
            {
                throw new ArgumentException("At least one bot metadata field must be supplied.");
            }
            Dictionary<string, string> body = PrepareBotMetadata(username, displayName, description);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Bot> DisableBotAsync(string botUserId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId)) + "/disable";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Bot> EnableBotAsync(string botUserId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId)) + "/enable";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Bot> AssignBotAsync(string botUserId, string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId))
                + "/assign/" + EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        private static Dictionary<string, string> PrepareBotMetadata(string? username, string? displayName, string? description)
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            if (username is string name)
            {
                if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("Bot username cannot be blank.", nameof(username)); }
                body["username"] = name.Trim();
            }
            if (displayName is string display) { body["display_name"] = display; }
            if (description is string text) { body["description"] = text; }
            return body;
        }
    }
}
