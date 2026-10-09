using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Helpers;
using Mattermost.Models.Channels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            return SendRequestAsync<Channel>(HttpMethod.Get, Routes.Channels + "/" + channelId);
        }

        /// <summary>
        /// Get a page of public channels for a team.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of channels per page. </param>
        /// <returns> Public channels for the team. </returns>
        public Task<IList<Channel>> GetTeamChannelsAsync(string teamId, int page = 0, int perPage = 60)
        {
            CheckDisposed();
            ValidateTeamIdentifier(teamId, nameof(teamId));
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            string url = Routes.Teams + "/" + Uri.EscapeDataString(teamId.Trim()) + "/channels?" + query;
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, url);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetUserChannelsAsync(string userId, bool includeDeleted = false,
            long lastDeleteAt = 0, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User identifier cannot be null or empty.", nameof(userId));
            }
            if (lastDeleteAt < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastDeleteAt), "Deletion timestamp cannot be negative.");
            }
            cancellationToken.ThrowIfCancellationRequested();

            string url = Routes.Users + "/" + Uri.EscapeDataString(userId.Trim()) + "/channels"
                + "?include_deleted=" + includeDeleted.ToString().ToLowerInvariant()
                + "&last_delete_at=" + lastDeleteAt.ToString(CultureInfo.InvariantCulture);
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<ChannelUserInfo>> GetChannelMembersAsync(string channelId, int page = 0,
            int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("Channel identifier cannot be null or empty.", nameof(channelId));
            }
            if (perPage > MattermostApiLimits.MaxChannelMembersPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), $"At most {MattermostApiLimits.MaxChannelMembersPerPage} members can be requested per page.");
            }
            cancellationToken.ThrowIfCancellationRequested();

            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            string url = Routes.Channels + "/" + Uri.EscapeDataString(channelId.Trim()) + "/members?" + query;
            return SendRequestAsync<IList<ChannelUserInfo>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelUserInfo> GetChannelMemberAsync(string channelId, string userId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("Channel identifier cannot be null or empty.", nameof(channelId));
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User identifier cannot be null or empty.", nameof(userId));
            }
            cancellationToken.ThrowIfCancellationRequested();

            string url = Routes.Channels + "/" + Uri.EscapeDataString(channelId.Trim())
                + "/members/" + Uri.EscapeDataString(userId.Trim());
            return SendRequestAsync<ChannelUserInfo>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<ChannelUserInfo>> GetChannelMembersByIdsAsync(string channelId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedChannelId = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareUserBatch(userIds, nameof(userIds));
            string url = Routes.Channels + "/" + escapedChannelId + "/members/ids";
            return SendRequestAsync<IList<ChannelUserInfo>>(HttpMethod.Post, url, ids, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelStats> GetChannelStatsAsync(string channelId, bool excludeFilesCount = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedChannelId = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Channels + "/" + escapedChannelId + "/stats";
            if (excludeFilesCount)
            {
                url += "?exclude_files_count=true";
            }
            return SendRequestAsync<ChannelStats>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelUnread> GetChannelUnreadAsync(string channelId, string userId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedChannelId = EscapeReadIdentifier(channelId, nameof(channelId));
            string escapedUserId = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Users + "/" + escapedUserId + "/channels/" + escapedChannelId + "/unread";
            return SendRequestAsync<ChannelUnread>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetUserTeamChannelsAsync(string userId, string teamId, bool includeDeleted = false,
            long lastDeleteAt = 0, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedUserId = EscapeReadIdentifier(userId, nameof(userId));
            string escapedTeamId = EscapeReadIdentifier(teamId, nameof(teamId));
            if (lastDeleteAt < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastDeleteAt), "Deletion timestamp cannot be negative.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Users + "/" + escapedUserId + "/teams/" + escapedTeamId + "/channels"
                + "?include_deleted=" + includeDeleted.ToString().ToLowerInvariant()
                + "&last_delete_at=" + lastDeleteAt.ToString(CultureInfo.InvariantCulture);
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<ChannelUserInfo>> GetUserTeamChannelMembersAsync(string userId, string teamId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedUserId = EscapeReadIdentifier(userId, nameof(userId));
            string escapedTeamId = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Users + "/" + escapedUserId + "/teams/" + escapedTeamId + "/channels/members";
            return SendRequestAsync<IList<ChannelUserInfo>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<string>> GetChannelTimezonesAsync(string channelId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedChannelId = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, Routes.Channels + "/" + escapedChannelId + "/timezones",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<string>());
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
            CheckDisposed();
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
            if (string.IsNullOrWhiteSpace(userId1))
            {
                throw new ArgumentException("User ID #1 cannot be null or empty.", nameof(userId1));
            }
            if (string.IsNullOrWhiteSpace(userId2))
            {
                throw new ArgumentException("User ID #2 cannot be null or empty.", nameof(userId2));
            }
            string[] body = new[] { userId1, userId2 };
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels + "/direct", body);
        }
    }
}
