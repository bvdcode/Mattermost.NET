using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Extensions;
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
        /// <summary>
        /// Login with specified login identifier and password.
        /// </summary>
        /// <param name="username">Username or email.</param>
        /// <param name="password">Password.</param>
        /// <returns>Authorized <see cref="User"/> object.</returns>
        /// <exception cref="AuthorizationException">Throws if credentials are invalid or server response is not successful.</exception>
        /// <exception cref="ArgumentException">Throws if username or password is empty.</exception>
        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username or password is empty");
            }
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new AuthorizationException("You cannot use API key and login with username/password at the same time");
            }

            CheckDisposed();
            var body = new
            {
                login_id = username,
                password
            };

            using HttpResponseMessage result = await SendHttpRequestAsync(
                HttpMethod.Post,
                Routes.Users + "/login",
                payload: body,
                requiresAuthorization: false).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new AuthorizationException("Login error, server response: " + result.StatusCode);
            }

            if (!result.Headers.TryGetValues("Token", out IEnumerable<string>? tokenValues))
            {
                throw new AuthorizationException("Token not found in response headers");
            }

            string? token = null;
            foreach (string value in tokenValues)
            {
                token = value;
                break;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AuthorizationException("Token not found in response headers");
            }

            _accessToken = token;
            _cachedUserInfo = await result.GetResponseAsync<User>().ConfigureAwait(false);
            return _cachedUserInfo.MemberwiseClone();
        }

        /// <summary>
        /// Logout from server.
        /// </summary>
        /// <returns>Task representing logout operation.</returns>
        /// <exception cref="MattermostClientException">Throws if server response is not successful.</exception>
        public async Task LogoutAsync()
        {
            CheckDisposed();
            await CheckAuthorizedAsync().ConfigureAwait(false);

            using HttpResponseMessage response = await SendHttpRequestAsync(
                HttpMethod.Post,
                Routes.Users + "/logout").ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new MattermostClientException("Logout error, server response: " + response.StatusCode);
            }

            await StopReceivingAsync().ConfigureAwait(false);
            _accessToken = null;
            _cachedUserInfo = null;
        }

        private Task CheckAuthorizedAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                string? apiKey = _apiKey;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    return LoginWithApiKeyAsync(apiKey!, cancellationToken);
                }

                throw new AuthorizationException("Authorization token is not set - call LoginAsync first or use constructor with API key (Personal Access Token)");
            }

            return Task.CompletedTask;
        }

        private async Task<User> LoginWithApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApiKeyException("API key is empty");
            }

            using HttpResponseMessage result = await SendHttpRequestAsync(
                HttpMethod.Get,
                Routes.Users + "/me",
                requiresAuthorization: false,
                authorizationTokenOverride: apiKey,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new ApiKeyException("Login with API key error, server response: " + result.StatusCode);
            }

            _cachedUserInfo = await result.GetResponseAsync<User>().ConfigureAwait(false);
            _accessToken = apiKey;
            return _cachedUserInfo;
        }
    }
}
