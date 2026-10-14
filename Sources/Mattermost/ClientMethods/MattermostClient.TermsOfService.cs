using Mattermost.Constants;
using Mattermost.Models;
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
        public Task<TermsOfService> GetTermsOfServiceAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TermsOfService>(HttpMethod.Get, Routes.TermsOfService, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<TermsOfService> CreateTermsOfServiceAsync(string text, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentException("Terms of service text cannot be blank.", nameof(text)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TermsOfService>(HttpMethod.Post, Routes.TermsOfService, new { text = text.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserTermsOfService> GetUserTermsOfServiceAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserTermsOfService>(HttpMethod.Get, Routes.Users + "/me/terms_of_service", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task RegisterTermsOfServiceActionAsync(string termsOfServiceId, bool accepted, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(termsOfServiceId)) { throw new ArgumentException("Terms of service identifier cannot be blank.", nameof(termsOfServiceId)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { termsOfServiceId = termsOfServiceId.Trim(), accepted };
            return SendRequestAsync(HttpMethod.Post, Routes.Users + "/me/terms_of_service", body, cancellationToken);
        }
    }
}
