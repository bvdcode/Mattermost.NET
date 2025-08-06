using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Mattermost.Enums;
using Mattermost.Constants;
using Mattermost.Exceptions;
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
        public Task<Channel> GetChannelAsync(string channelId)
        {
            CheckDisposed();
            CheckAuthorized();
            return SendRequestAsync<Channel>(HttpMethod.Get, Routes.Channels + "/" + channelId);
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
            try
            {
                return await SendRequestAsync<Channel>(HttpMethod.Get, url);
            }
            catch (MattermostClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Archive channel by specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        public Task ArchiveChannelAsync(string channelId)
        {
            CheckDisposed();
            CheckAuthorized();
            return SendRequestAsync(HttpMethod.Delete, Routes.Channels + "/" + channelId);
        }

        /// <summary>
        /// Add user to channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Channel user information. </returns>
        public Task<ChannelUserInfo> AddUserToChannelAsync(string channelId, string userId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + $"/{channelId}/members";
            var body = new
            {
                user_id = userId
            };
            return SendRequestAsync<ChannelUserInfo>(HttpMethod.Post, url, body);
        }

        /// <summary>
        /// Delete user from channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        public Task DeleteUserFromChannelAsync(string channelId, string userId)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Channels + $"/{channelId}/members/{userId}";
            return SendRequestAsync(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Create group channel with specified users.
        /// </summary>
        /// <param name="userIds"> Participant users. </param>
        /// <returns> Created channel info. </returns>
        public Task<Channel> CreateGroupChannelAsync(params string[] userIds)
        {
            CheckDisposed();
            CheckAuthorized();
            if (userIds.Length < 2)
            {
                throw new ArgumentException("At least two user IDs are required to create a group channel.", nameof(userIds));
            }
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.GroupChannels, userIds);
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
        public Task<Channel> CreateChannelAsync(string teamId, string name, string displayName,
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
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels, body);
        }

        /// <summary>
        /// Create a new direct message channel between two users. <br/>
        /// Must be one of the two users and have create_direct_channel permission. <br/>
        /// Having the manage_system permission voids the previous requirements.
        /// </summary>
        /// <param name="userId"> User identifier to create direct channel with. </param>
        /// <returns>Created direct channel.</returns>
        public Task<Channel> CreateDirectChannelAsync(string userId)
        {
            return CreateDirectChannelAsync(CurrentUserInfo.Id, userId);
        }

        /// <summary>
        /// Create a new direct message channel between two users. <br/>
        /// Must be one of the two users and have create_direct_channel permission. <br/>
        /// Having the manage_system permission voids the previous requirements.
        /// </summary>
        /// <param name="userId1"> First user identifier to create direct channel with. </param>
        /// <param name="userId2"> Second user identifier to create direct channel with. </param>
        /// <returns>Created direct channel.</returns>
        public Task<Channel> CreateDirectChannelAsync(string userId1, string userId2)
        {
            CheckDisposed();
            CheckAuthorized();
            if (string.IsNullOrWhiteSpace(userId1))
            {
                throw new ArgumentException("User ID #1 cannot be null or empty.", nameof(userId1));
            }
            if (string.IsNullOrWhiteSpace(userId2))
            {
                throw new ArgumentException("User ID #2 cannot be null or empty.", nameof(userId2));
            }
            var body = new[] { userId1, userId2 };
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels + "/direct", body);
        }
    }
}
