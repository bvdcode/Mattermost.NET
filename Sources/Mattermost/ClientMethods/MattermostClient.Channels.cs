using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Mattermost.Enums;
using Mattermost.Constants;
using System.Threading.Tasks;
using Mattermost.Models.Channels;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get channel from the provided channel id string.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> Channel information. </returns>
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + "/" + channelId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Channel>(json)
                ?? throw new JsonException("Failed to deserialize channel information.");
        }

        /// <summary>
        /// Find channel by channel name and team name or identifier.
        /// </summary>
        /// <param name="teamIdOrName"> Team name or identifier where channel exists. </param>
        /// <param name="channelName"> Channel name. </param>
        /// <param name="isTeamId"> True if teamIdOrName is team identifier, otherwise false (team name). Default is true. </param>
        /// <param name="includeDeleted"> Include deleted channels in search, default is true. </param>
        /// <returns> Channel info, or null if team or channel not found. </returns>
        public async Task<Channel?> FindChannelByNameAsync(string teamIdOrName, string channelName, bool isTeamId = true, bool includeDeleted = true)
        {
            CheckDisposed();
            CheckAuthorized();
            string escapedTeamIdOrName = Uri.EscapeDataString(teamIdOrName);
            string escapedChannelName = Uri.EscapeDataString(channelName);
            string url = isTeamId
                ? Routes.Teams + $"/{escapedTeamIdOrName}/channels/name/{escapedChannelName}?include_deleted={includeDeleted}"
                : Routes.Teams + $"/name/{escapedTeamIdOrName}/channels/name/{escapedChannelName}?include_deleted={includeDeleted}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Channel>(json);
        }

        /// <summary>
        /// Archive channel by specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        public async Task<bool> ArchiveChannelAsync(string channelId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + "/" + channelId;
            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add user to channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Channel user information. </returns>
        public async Task<ChannelUserInfo> AddUserToChannelAsync(string channelId, string userId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + $"/{channelId}/members";
            var body = new
            {
                user_id = userId
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChannelUserInfo>(json)!;
        }

        /// <summary>
        /// Delete user from channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        public async Task<bool> DeleteUserFromChannelAsync(string channelId, string userId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + $"/{channelId}/members/{userId}";
            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Create group channel with specified users.
        /// </summary>
        /// <param name="userIds"> Participant users. </param>
        /// <returns> Created channel info. </returns>
        public async Task<Channel> CreateGroupChannelAsync(params string[] userIds)
        {
            CheckDisposed();
            CheckAuthorized();
            string json = JsonSerializer.Serialize(userIds);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Routes.GroupChannels, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Channel>(json)!;

            //TODO: create channel link
            var team = await GetTeamAsync(result.TeamId);
            result.Link = ServerAddress + team.Name + "/channels/" + result.Id;
            //

            return result;
        }

        /// <summary>
        /// Create simple channel with specified users.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <param name="name"> Channel name. </param>
        /// <param name="displayName"> Channel display name. </param>
        /// <param name="channelType"> Channel type: open or private. </param>
        /// <param name="purpose"> Channel purpose (optional). </param>
        /// <param name="header"> Channel header (optional). </param>
        /// <returns> Created channel info. </returns>
        public async Task<Channel> CreateChannelAsync(string teamId, string name, string displayName,
            ChannelType channelType, string purpose = "", string header = "")
        {
            CheckDisposed();
            CheckAuthorized();
            const int maxChannelDisplayNameLength = 64;
            if (displayName.Length > maxChannelDisplayNameLength)
            {
                throw new ArgumentException("Display name is too long", nameof(displayName));
            }
            var body = new
            {
                team_id = teamId,
                name,
                display_name = displayName,
                purpose,
                header,
                type = channelType.ToChannelChar()
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Routes.Channels, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Channel>(json)!;
            var team = await GetTeamAsync(teamId);
            result.Link = ServerAddress + team.Name + "/channels/" + result.Id;
            return result;
        }
    }
}
