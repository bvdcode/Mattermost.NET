using Mattermost.Constants;
using Mattermost.Models.Users;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task RevokeUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(tokenId)) { throw new ArgumentException("Token ID cannot be blank.", nameof(tokenId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.UserAccessTokens + "/revoke", new { token_id = tokenId.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserAccessToken> RotateUserAccessTokenAsync(string tokenId, long expiresAt = 0, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(tokenId)) { throw new ArgumentException("Token ID cannot be blank.", nameof(tokenId)); }
            if (expiresAt < 0) { throw new ArgumentOutOfRangeException(nameof(expiresAt)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { token_id = tokenId.Trim(), expires_at = expiresAt };
            return SendRequestAsync<UserAccessToken>(HttpMethod.Post, Routes.UserAccessTokens + "/rotate", body, cancellationToken);
        }
    }
}
