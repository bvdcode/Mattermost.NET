using Mattermost.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(email)) { throw new ArgumentException("An email address is required.", nameof(email)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendUnauthenticatedRequestAsync(HttpMethod.Post, Routes.Users + "/password/reset/send", new { email = email.Trim() },
                cancellationToken);
        }

        /// <inheritdoc />
        public Task SendVerificationEmailAsync(string email, string? redirect = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(email)) { throw new ArgumentException("An email address is required.", nameof(email)); }
            string route = Routes.Users + "/email/verify/send";
            if (redirect is string target) { route += "?r=" + Uri.EscapeDataString(target.Trim()); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendUnauthenticatedRequestAsync(HttpMethod.Post, route, new { email = email.Trim() }, cancellationToken);
        }
    }
}
