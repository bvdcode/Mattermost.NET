using Mattermost.Constants;
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
        public Task<User> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(username)) { throw new ArgumentException("Username cannot be blank.", nameof(username)); }
            if (string.IsNullOrWhiteSpace(email)) { throw new ArgumentException("Email cannot be blank.", nameof(email)); }
            if (string.IsNullOrEmpty(password)) { throw new ArgumentException("Password cannot be empty.", nameof(password)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { username = username.Trim(), email = email.Trim(), password };
            return SendRequestAsync<User>(HttpMethod.Post, Routes.Users, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<User> PatchUserAsync(string userId, UserPatch patch, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/patch";
            if (patch is null) { throw new ArgumentNullException(nameof(patch)); }
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "username", TrimRequiredUserPatchText(patch.Username, nameof(patch.Username)));
            AddPatchValue(body, "email", TrimRequiredUserPatchText(patch.Email, nameof(patch.Email)));
            AddPatchValue(body, "locale", TrimRequiredUserPatchText(patch.Locale, nameof(patch.Locale)));
            AddPatchValue(body, "nickname", patch.Nickname?.Trim());
            AddPatchValue(body, "first_name", patch.FirstName?.Trim());
            AddPatchValue(body, "last_name", patch.LastName?.Trim());
            AddPatchValue(body, "position", patch.Position?.Trim());
            AddPatchValue(body, "timezone", patch.Timezone);
            AddPatchValue(body, "props", patch.Props);
            AddPatchValue(body, "notify_props", patch.NotifyProps);
            if (body.Count == 0) { throw new ArgumentException("At least one profile field must be supplied.", nameof(patch)); }
            if (patch.Password is string password)
            {
                if (patch.Email is null || password.Length == 0)
                {
                    throw new ArgumentException("A nonempty confirmation password requires an email change.", nameof(patch));
                }
                body.Add("password", password);
            }
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement snapshot = JsonSerializer.SerializeToElement(body);
            return SendRequestAsync<User>(HttpMethod.Put, route, snapshot, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateUserActiveAsync(string userId, bool active, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/active";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { active }, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateUserRolesAsync(string userId, string roles, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/roles";
            if (string.IsNullOrWhiteSpace(roles)) { throw new ArgumentException("Roles cannot be blank.", nameof(roles)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { roles = roles.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateUserPasswordAsync(string userId, string newPassword, string? currentPassword = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/password";
            if (string.IsNullOrEmpty(newPassword)) { throw new ArgumentException("New password cannot be empty.", nameof(newPassword)); }
            if (currentPassword is string current && current.Length == 0)
            {
                throw new ArgumentException("Current password cannot be empty when supplied.", nameof(currentPassword));
            }
            Dictionary<string, string> body = new Dictionary<string, string> { ["new_password"] = newPassword };
            if (currentPassword is string password) { body.Add("current_password", password); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task ResetUserFailedAttemptsAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/reset_failed_attempts";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<User> VerifyUserEmailAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/email/verify/member";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<User>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task RevokeUserSessionAsync(string userId, string sessionId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/sessions/revoke";
            if (string.IsNullOrWhiteSpace(sessionId)) { throw new ArgumentException("Session identifier cannot be blank.", nameof(sessionId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, route, new { session_id = sessionId.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task RevokeAllUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/sessions/revoke/all";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        private static string? TrimRequiredUserPatchText(string? value, string parameterName)
        {
            if (value is null) { return null; }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("Profile identifier cannot be blank.", parameterName); }
            return value.Trim();
        }
    }
}
