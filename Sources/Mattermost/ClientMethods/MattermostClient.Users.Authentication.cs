using Mattermost.Constants;
using Mattermost.Models.Users;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<User> GetUserByAuthDataAsync(string authData, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateUserAuthData(authData);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/auth_data?value=" + Uri.EscapeDataString(authData),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserAuthentication> UpdateUserAuthenticationAsync(string userId, string authService, string? authData = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string user = EscapeReadIdentifier(userId, nameof(userId));
            if (string.IsNullOrWhiteSpace(authService)) { throw new ArgumentException("Authentication service cannot be blank.", nameof(authService)); }
            string service = authService.Trim();
            if (service == "email")
            {
                if (authData is object) { throw new ArgumentException("Email authentication requires null authentication data.", nameof(authData)); }
            }
            else { ValidateUserAuthData(authData); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserAuthentication>(HttpMethod.Put, Routes.Users + "/" + user + "/auth",
                new { auth_service = service, auth_data = authData }, cancellationToken);
        }

        private static void ValidateUserAuthData(string? authData)
        {
            if (string.IsNullOrEmpty(authData) || Encoding.UTF8.GetByteCount(authData) > MattermostApiLimits.MaxUserAuthDataBytes)
            {
                throw new ArgumentException($"Authentication data must contain 1 through {MattermostApiLimits.MaxUserAuthDataBytes} UTF-8 bytes.", nameof(authData));
            }
        }
    }
}
