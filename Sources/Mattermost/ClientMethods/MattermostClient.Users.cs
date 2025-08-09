using System.Net.Http;
using System.Text.Json;
using Mattermost.Constants;
using System.Threading.Tasks;
using Mattermost.Models.Users;
using System;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get current authorized user information and force update of <see cref="CurrentUserInfo"/>.
        /// </summary>
        /// <returns> Authorized user information. </returns>
        public async Task<User> GetMeAsync()
        {
            CheckDisposed();
            _cachedUserInfo = await SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/me");
            return _cachedUserInfo.MemberwiseClone();
        }

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserAsync(string userId)
        {
            CheckDisposed();
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            return SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/" + userId);
        }

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserByUsernameAsync(string username)
        {
            CheckDisposed();
            string sanitizedUsername = username.Replace("@", string.Empty).Trim();
            if (string.IsNullOrEmpty(sanitizedUsername))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }
            return SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/username/" + sanitizedUsername);
        }

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email"> Email address. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserByEmailAsync(string email)
        {
            CheckDisposed();
            string url = Routes.Users + "/email/" + email.Trim();
            return SendRequestAsync<User>(HttpMethod.Get, url);
        }
    }
}
