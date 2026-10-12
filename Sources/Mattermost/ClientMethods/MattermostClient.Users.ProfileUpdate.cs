using Mattermost.Constants;
using Mattermost.Models.Users;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<User> UpdateUserAsync(string userId, UserProfileUpdate profile, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string identifier = EscapeReadIdentifier(userId, nameof(userId));
            if (userId.Trim() == "me") { throw new ArgumentException("An actual user identifier is required.", nameof(userId)); }
            if (profile is null) { throw new ArgumentNullException(nameof(profile)); }
            string username = RequiredUserProfileText(profile.Username, nameof(profile.Username));
            string email = RequiredUserProfileText(profile.Email, nameof(profile.Email));
            string locale = RequiredUserProfileText(profile.Locale, nameof(profile.Locale));
            if (profile.Password is string password && password.Length == 0)
            {
                throw new ArgumentException("A supplied confirmation password cannot be empty.", nameof(profile));
            }
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement body = JsonSerializer.SerializeToElement(new
            {
                id = userId.Trim(), username, email, locale,
                nickname = profile.Nickname?.Trim() ?? string.Empty,
                first_name = profile.FirstName?.Trim() ?? string.Empty,
                last_name = profile.LastName?.Trim() ?? string.Empty,
                position = profile.Position?.Trim() ?? string.Empty,
                password = profile.Password,
                timezone = profile.Timezone,
                props = profile.Props,
                notify_props = profile.NotifyProps,
                allow_marketing = profile.AllowMarketing
            });
            return SendRequestAsync<User>(HttpMethod.Put, Routes.Users + "/" + identifier, body, cancellationToken);
        }

        private static string RequiredUserProfileText(string? value, string parameterName)
        {
            if (value is null || string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("A complete profile requires a nonblank value.", parameterName); }
            return value.Trim();
        }
    }
}
