using System.Text.Json;
using Mattermost.Constants;
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
            _userInfo = JsonSerializer.Deserialize<User>(json)!;
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
            User userInfo = JsonSerializer.Deserialize<User>(json)!;
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
            User userInfo = JsonSerializer.Deserialize<User>(json)!;
            return userInfo;
        }
    }
}
