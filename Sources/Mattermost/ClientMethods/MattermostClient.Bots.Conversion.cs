using Mattermost.Constants;
using Mattermost.Models.Bots;
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
        public Task<Bot> ConvertUserToBotAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/convert_to_bot";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Bot>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<User> ConvertBotToUserAsync(string botUserId, string password, string? email = null,
            bool setSystemAdmin = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Bots + "/" + EscapeReadIdentifier(botUserId, nameof(botUserId))
                + "/convert_to_user?set_system_admin=" + setSystemAdmin.ToString().ToLowerInvariant();
            if (string.IsNullOrEmpty(password)) { throw new ArgumentException("A password is required.", nameof(password)); }
            Dictionary<string, string> body = new Dictionary<string, string> { ["password"] = password };
            if (email is string address)
            {
                if (string.IsNullOrWhiteSpace(address)) { throw new ArgumentException("Email cannot be blank when supplied.", nameof(email)); }
                body.Add("email", address.Trim());
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<User>(HttpMethod.Post, route, body, cancellationToken);
        }
    }
}
