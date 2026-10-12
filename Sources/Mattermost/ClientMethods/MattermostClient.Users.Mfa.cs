using Mattermost.Constants;
using Mattermost.Models.Users;
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
        public Task<MfaSecret> GenerateMfaSecretAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/mfa/generate";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<MfaSecret>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateUserMfaAsync(string userId, bool activate, string? code = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/mfa";
            Dictionary<string, object> body = new Dictionary<string, object> { ["activate"] = activate };
            if (activate)
            {
                if (code is null || code.Length == 0) { throw new ArgumentException("A code is required to activate MFA.", nameof(code)); }
                body["code"] = code;
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, body, cancellationToken);
        }
    }
}
