using System.Text.Json;
using Mattermost.Constants;
using Mattermost.Exceptions;
using System.Threading.Tasks;
using Mattermost.Models.Users;

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
            CheckAuthorized();
            var response = await _http.GetAsync(Routes.Users + "/me");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            _userInfo = JsonSerializer.Deserialize<User>(json)
                ?? throw new JsonException("Failed to deserialize user information: " + json);
            return _userInfo;
        }

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        public async Task<User> GetUserAsync(string userId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Users + "/" + userId;
            string json = await _http.GetStringAsync(url);
            User userInfo = JsonSerializer.Deserialize<User>(json)
                ?? throw new JsonException($"Failed to deserialize user information for ID#{userId}: {json}");
            return userInfo;
        }

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Users + "/username/" + username.Replace("@", string.Empty).Trim();
            string json = await _http.GetStringAsync(url);
            User userInfo = JsonSerializer.Deserialize<User>(json)
                ?? throw new JsonException($"Failed to deserialize user information for username '{username}': {json}");
            return userInfo;
        }

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email"> Email address. </param>
        /// <returns> User information. </returns>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Users + "/email/" + email.Trim();
            var response = await _http.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new MattermostClientException("Access to user information by email is forbidden. Check your server settings or permissions.");
            }
            string json = await response.Content.ReadAsStringAsync();
            User userInfo = JsonSerializer.Deserialize<User>(json)
                ?? throw new JsonException($"Failed to deserialize user information for email '{email}': {json}");
            return userInfo;
        }
    }
}
