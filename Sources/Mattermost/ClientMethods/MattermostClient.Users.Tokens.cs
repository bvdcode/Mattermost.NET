using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Users;
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
        public Task<UserAccessToken> CreateUserAccessTokenAsync(string userId, string description, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/tokens";
            if (string.IsNullOrWhiteSpace(description)) { throw new ArgumentException("Token description cannot be blank.", nameof(description)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { description = description.Trim() };
            return SendRequestAsync<UserAccessToken>(HttpMethod.Post, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<UserAccessToken>> GetUserAccessTokensAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/tokens?"
                + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UserAccessToken>>(HttpMethod.Get, route,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<UserAccessToken>());
        }

        /// <inheritdoc />
        public Task<IList<UserAccessToken>> GetAllUserAccessTokensAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.UserAccessTokens + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UserAccessToken>>(HttpMethod.Get, route,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<UserAccessToken>());
        }

        /// <inheritdoc />
        public Task<UserAccessToken> GetUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.UserAccessTokens + "/" + EscapeReadIdentifier(tokenId, nameof(tokenId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserAccessToken>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<UserAccessToken>> SearchUserAccessTokensAsync(string term, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(term)) { throw new ArgumentException("Search term cannot be blank.", nameof(term)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { term = term.Trim() };
            return SendRequestAsync<IList<UserAccessToken>>(HttpMethod.Post, Routes.UserAccessTokens + "/search", body, cancellationToken,
                nullResultFactory: () => new List<UserAccessToken>());
        }

        /// <inheritdoc />
        public Task DisableUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(tokenId)) { throw new ArgumentException("Token ID cannot be blank.", nameof(tokenId)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { token_id = tokenId.Trim() };
            return SendRequestAsync(HttpMethod.Post, Routes.UserAccessTokens + "/disable", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task EnableUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(tokenId)) { throw new ArgumentException("Token ID cannot be blank.", nameof(tokenId)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { token_id = tokenId.Trim() };
            return SendRequestAsync(HttpMethod.Post, Routes.UserAccessTokens + "/enable", body, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> GetNonCompliantUserAccessTokenCountAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.UserAccessTokens + "/non_compliant/count",
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("count", out JsonElement count)
                || count.ValueKind != JsonValueKind.Number || !count.TryGetInt64(out long value))
            {
                throw new JsonException("Invalid token policy count response.");
            }
            return value;
        }
    }
}
