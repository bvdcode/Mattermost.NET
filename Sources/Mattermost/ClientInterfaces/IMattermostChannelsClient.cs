using Mattermost.Enums;
using Mattermost.Constants;
using Mattermost.Models.Channels;
using Mattermost.Models.Responses;
using Mattermost.Models.Teams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Channels, channel membership, sidebar categories and Calls plugin operations.
    /// </summary>
    public interface IMattermostChannelsClient
    {
        /// <summary>Read an administrative page of public and private channels across teams.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Between one and MaxChannelsPerPage results.</param>
        /// <param name="includeDeleted">Include archived channels.</param>
        /// <param name="excludeDefaultChannels">Exclude configured default channels.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>A channel page with team metadata and the total matching count.</returns>
        /// <remarks>Requires a system console permission to read channels, manage groups, or read retention policies. This is not a user-membership listing.</remarks>
        Task<ChannelListResponse> GetAllChannelsAsync(int page = 0, int perPage = 60, bool includeDeleted = false,
            bool excludeDefaultChannels = false, CancellationToken cancellationToken = default);

        /// <summary>Search public and private channels across teams in administrative mode.</summary>
        /// <param name="term">Search text; empty matches all channels. Surrounding whitespace is removed.</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Between one and MaxChannelsPerPage results.</param>
        /// <param name="teamIds">Optional team identifiers. Team filtering requires Mattermost 5.26 or later.</param>
        /// <param name="channelType">Public, Private, or null for both.</param>
        /// <param name="includeDeleted">Include archived channels.</param>
        /// <param name="excludeDefaultChannels">Exclude configured default channels.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>A channel page with team metadata and the total matching count.</returns>
        /// <remarks>Requires a system console permission to read channels, manage groups, or read retention policies. Uses system_console=true and native JSON paging fields.</remarks>
        Task<ChannelListResponse> SearchAllChannelsAsync(string term, int page = 0, int perPage = 60,
            IEnumerable<string>? teamIds = null, ChannelType? channelType = null, bool includeDeleted = false,
            bool excludeDefaultChannels = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get channel from the provided channel id string.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> Channel information. </returns>
        Task<Channel> GetChannelAsync(string channelId);

        /// <summary>
        /// Get a page of public channels for a team.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of channels per page. </param>
        /// <returns> Public channels for the team. </returns>
        Task<IList<Channel>> GetTeamChannelsAsync(string teamId, int page = 0, int perPage = 60);

        /// <summary>
        /// Get all channels the user belongs to across all teams. Requires Mattermost 6.1 or later.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="includeDeleted">Whether to include archived channels.</param>
        /// <param name="lastDeleteAt">Unix timestamp in milliseconds used to filter archived channels. Ignored unless includeDeleted is true.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Channels the user belongs to, including direct and group channels.</returns>
        /// <remarks>Requires the current user's own identifier or permission to edit other users.</remarks>
        /// <exception cref="ArgumentException">The user identifier is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The deletion timestamp is negative.</exception>
        Task<IList<Channel>> GetUserChannelsAsync(string userId, bool includeDeleted = false,
            long lastDeleteAt = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get one page of channel memberships. Requires permission to read the channel.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Number of members per page, from 1 to <see cref="MattermostApiLimits.MaxChannelMembersPerPage"/>.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Channel memberships, not user profiles.</returns>
        /// <exception cref="ArgumentException">The channel identifier is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The page is negative or the page size is outside the allowed range.</exception>
        Task<IList<ChannelUserInfo>> GetChannelMembersAsync(string channelId, int page = 0,
            int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's membership in a channel. Requires permission to read the channel.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The channel membership, not the user's profile.</returns>
        /// <exception cref="ArgumentException">A channel or user identifier is blank.</exception>
        Task<ChannelUserInfo> GetChannelMemberAsync(string channelId, string userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get channel memberships by user identifiers in one request. Requires permission to read the channel.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userIds">A nonempty collection of actual user identifiers, not "me".</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible memberships, not user profiles. Missing members may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The channel identifier is blank, or the collection is empty or contains blank identifiers.</exception>
        Task<IList<ChannelUserInfo>> GetChannelMembersByIdsAsync(string channelId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get statistics for a channel. Requires permission to read the channel.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="excludeFilesCount">Skip counting files; supporting servers return -1 for the file count.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Member, guest, pinned post and file counts.</returns>
        /// <exception cref="ArgumentException">The channel identifier is blank.</exception>
        Task<ChannelStats> GetChannelStatsAsync(string channelId, bool excludeFilesCount = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's unread counts for a channel. Requires permission to access the user and read the channel.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Unread message and mention counts. This does not mark messages as read.</returns>
        /// <exception cref="ArgumentException">A channel or user identifier is blank.</exception>
        Task<ChannelUnread> GetChannelUnreadAsync(string channelId, string userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's channels in one team. Requires permission to access the user and view the team.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="includeDeleted">Include archived channels.</param>
        /// <param name="lastDeleteAt">Unix timestamp in milliseconds filtering archived channels when includeDeleted is true.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The user's channels in the team, including direct and group message channels.</returns>
        /// <exception cref="ArgumentException">A user or team identifier is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The deletion timestamp is negative.</exception>
        Task<IList<Channel>> GetUserTeamChannelsAsync(string userId, string teamId, bool includeDeleted = false,
            long lastDeleteAt = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's channel memberships in one team. Requires permission to view the team.
        /// Reading another user's memberships requires system administrator permissions.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Membership roles, counters and notification settings, including direct and group message memberships.</returns>
        /// <exception cref="ArgumentException">A user or team identifier is blank.</exception>
        Task<IList<ChannelUserInfo>> GetUserTeamChannelMembersAsync(string userId, string teamId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get timezones used by channel members. Requires permission to read the channel and Mattermost 5.6 or later.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Timezone names reported by the server, or an empty list when members have no timezone configured.</returns>
        /// <exception cref="ArgumentException">The channel identifier is blank.</exception>
        Task<IList<string>> GetChannelTimezonesAsync(string channelId, CancellationToken cancellationToken = default);

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
        Task<Channel> CreateChannelAsync(string teamId, string name, string displayName,
            ChannelType channelType, string purpose = "", string header = "");

        /// <summary>
        /// Create group channel with specified users.
        /// </summary>
        /// <param name="userIds"> Participant users. </param>
        /// <returns> Created channel info. </returns>
        Task<Channel> CreateGroupChannelAsync(params string[] userIds);

        /// <summary>Move a public or private channel to another team. Requires Mattermost 5.26 or later.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="teamId">Destination team identifier.</param>
        /// <param name="force">Remove channel members who do not belong to the destination team before moving.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The moved channel with its new team identifier.</returns>
        /// <remarks>Requires manage_system. Direct and group channels cannot be moved. The server updates related threads, sidebar categories and channel webhooks. Deactivated members may be removed even when force is false; an error does not guarantee rollback.</remarks>
        Task<Channel> MoveChannelAsync(string channelId, string teamId, bool force = false,
            CancellationToken cancellationToken = default);

        /// <summary>Convert a group message into a private channel in a common team.</summary>
        /// <param name="channelId">Group message channel identifier.</param>
        /// <param name="teamId">Team shared by all group participants.</param>
        /// <param name="name">New channel URL name.</param>
        /// <param name="displayName">New channel display name.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The converted channel, retaining its identifier and messages.</returns>
        /// <remarks>Requires group membership and create_private_channel in the destination team. Guests cannot convert group messages. The current user becomes channel administrator. This does not create a copy of the group message.</remarks>
        Task<Channel> ConvertGroupMessageToChannelAsync(string channelId, string teamId, string name, string displayName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add user to channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Channel user information. </returns>
        Task<ChannelUserInfo> AddUserToChannelAsync(string channelId, string userId);

        /// <summary>
        /// Delete user from channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        Task DeleteUserFromChannelAsync(string channelId, string userId);

        /// <summary>
        /// Find channel by channel name and team name or identifier.
        /// </summary>
        /// <param name="teamIdOrName"> Team name or identifier where channel exists. </param>
        /// <param name="channelName"> Channel name. </param>
        /// <param name="isTeamId"> True if teamIdOrName is team identifier, otherwise false (team name). Default is true. </param>
        /// <param name="includeDeleted"> Include deleted channels in search, default is true. </param>
        /// <returns> Channel info. </returns>
        Task<Channel?> FindChannelByNameAsync(string teamIdOrName, string channelName, bool isTeamId = true, bool includeDeleted = true);

        /// <summary>
        /// Archive channel by specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> True if archieved, otherwise false. </returns>
        Task ArchiveChannelAsync(string channelId);

        /// <summary>
        /// Create a new direct message channel between current user and specified user. <br/>
        /// Must have create_direct_channel permission. <br/>
        /// Having the manage_system permission voids the previous requirements.
        /// </summary>
        /// <param name="userId"> User identifier to create direct channel with. </param>
        /// <returns>Created direct channel.</returns>
        Task<Channel> CreateDirectChannelAsync(string userId);

        /// <summary>
        /// Create a new direct message channel between two users. <br/>
        /// Must be one of the two users and have create_direct_channel permission. <br/>
        /// Having the manage_system permission voids the previous requirements.
        /// </summary>
        /// <param name="userId1"> First user identifier to create direct channel with. </param>
        /// <param name="userId2"> Second user identifier to create direct channel with. </param>
        /// <returns>Created direct channel.</returns>
        Task<Channel> CreateDirectChannelAsync(string userId1, string userId2);

        /// <summary>
        /// Notifies a channel that the current user is typing. Requires Mattermost 5.26 or later.
        /// </summary>
        /// <param name="channelId">Channel to notify.</param>
        /// <param name="parentId">Root post ID for a thread, or null or empty for the channel.</param>
        /// <param name="cancellationToken">Cancels authentication and the notification request.</param>
        /// <returns>A task that completes when the server accepts the notification.</returns>
        /// <remarks>Uses the REST API and does not require StartReceivingAsync. Repeat while the user is typing.</remarks>
        /// <exception cref="ArgumentException">The channel ID is null, empty, or whitespace.</exception>
        Task SendTypingAsync(string channelId, string? parentId = null, CancellationToken cancellationToken = default);

        /// <summary>Update channel name, display name, header, and purpose from a channel object.</summary>
        /// <param name="channel">Channel with an actual Id. Header and purpose replace the stored values, including empty strings.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Channel> UpdateChannelAsync(Channel channel, CancellationToken cancellationToken = default);

        /// <summary>Update selected channel metadata. Null omits a field; an empty string is sent to the server.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="name">New URL name.</param>
        /// <param name="displayName">New display name.</param>
        /// <param name="header">New header; an empty string clears it.</param>
        /// <param name="purpose">New purpose; an empty string clears it.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Channel> PatchChannelAsync(string channelId, string? name = null, string? displayName = null,
            string? header = null, string? purpose = null, CancellationToken cancellationToken = default);

        /// <summary>Convert a channel to public or private. Requires the corresponding conversion permission. Requires Mattermost 5.16 or later.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="privacy">Public or Private. Direct, Group, and invalid enum values are rejected.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Channel> UpdateChannelPrivacyAsync(string channelId, ChannelType privacy, CancellationToken cancellationToken = default);

        /// <summary>Restore an archived channel. Requires permission to manage its team.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Channel> RestoreChannelAsync(string channelId, CancellationToken cancellationToken = default);

        /// <summary>Get a page of archived channels visible to the current user. Requires list_team_channels.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Items per page, from 1 to 200.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Channel>> GetDeletedChannelsAsync(string teamId, int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get a page of private team channels. Requires manage_system and Mattermost 5.26 or later.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Items per page, from 1 to 200.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Channel>> GetPrivateChannelsAsync(string teamId, int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Read public channels in a team by ID. Requires view_team.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="channelIds">Nonempty collection of channel identifiers.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching public channels. Order is not preserved; no matches produces a server error.</returns>
        Task<IList<Channel>> GetPublicChannelsByIdsAsync(string teamId, IEnumerable<string> channelIds, CancellationToken cancellationToken = default);

        /// <summary>Search public team channels by name or display name. Membership restrictions apply.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="term">Search term; surrounding whitespace is removed. Empty terms are allowed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Channel>> SearchTeamChannelsAsync(string teamId, string term, CancellationToken cancellationToken = default);

        /// <summary>Replace a member's channel roles. Requires manage_channel_roles permission in the channel.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="roles">Space-delimited roles, passed unchanged. The server validates role names.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateChannelMemberRolesAsync(string channelId, string userId, string roles, CancellationToken cancellationToken = default);

        /// <summary>Replace a member's scheme-derived channel roles. Requires Mattermost 5.0 or later and manage_channel_roles permission.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="schemeUser">Whether the member has the scheme user role.</param>
        /// <param name="schemeAdmin">Whether the member has the scheme administrator role.</param>
        /// <param name="schemeGuest">Whether the member has the scheme guest role, mutually exclusive with schemeUser.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateChannelMemberSchemeRolesAsync(string channelId, string userId, bool schemeUser, bool schemeAdmin,
            bool schemeGuest = false, CancellationToken cancellationToken = default);

        /// <summary>Update supplied channel notification properties. Omitted properties are preserved.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="userId">User identifier, or me for the current user. Other users require edit_other_users permission.</param>
        /// <param name="notifyProps">String-valued settings such as desktop, push, email, mark_unread and ignore_channel_mentions. Values and keys are passed unchanged; unsupported keys may be ignored by the server.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateChannelMemberNotifyPropsAsync(string channelId, string userId, IDictionary<string, string> notifyProps,
            CancellationToken cancellationToken = default);

        /// <summary>Get member counts for several accessible channels.</summary>
        /// <param name="channelIds">Nonempty channel identifier collection. Match results by channel ID, not input order. Missing channels may be omitted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IDictionary<string, long>> GetChannelsMemberCountsAsync(IEnumerable<string> channelIds, CancellationToken cancellationToken = default);

        /// <summary>Perform the server's channel-view action: mark channels read, clear push notifications and update the active channel.</summary>
        /// <param name="userId">User identifier, or me for the current user. Other users require edit_other_users permission.</param>
        /// <param name="channelId">Viewed channel identifier. An empty string explicitly reports loss of focus.</param>
        /// <param name="previousChannelId">Previous channel identifier when switching channels. Null omits the field; empty is allowed.</param>
        /// <param name="collapsedThreadsSupported">Whether the caller supports collapsed reply threads.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Operation status and view timestamps, available since Mattermost 4.3. Timestamps are Unix milliseconds.</returns>
        Task<ChannelViewResponse> ViewChannelAsync(string userId, string channelId, string? previousChannelId = null,
            bool collapsedThreadsSupported = false, CancellationToken cancellationToken = default);

        /// <summary>Mark several channels as read for a user. This changes unread counters and can update followed thread read state.</summary>
        /// <param name="userId">User identifier, or me for the current user. Other users require edit_other_users permission.</param>
        /// <param name="channelIds">Nonempty collection of accessible channel identifiers.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Operation status and Unix-millisecond view times keyed by channel identifier.</returns>
        Task<ChannelViewResponse> MarkChannelsReadAsync(string userId, IEnumerable<string> channelIds, CancellationToken cancellationToken = default);

        /// <summary>Search the current user's group channels by member names.</summary>
        /// <param name="term">Nonblank search term. Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Channel>> SearchGroupChannelsAsync(string term, CancellationToken cancellationToken = default);

        /// <summary>Get a user's sidebar categories and their order. Requires Mattermost 5.26 or later and access to the team.</summary>
        /// <param name="userId">User identifier, or me for the current user. Other users require edit_other_users permission.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OrderedSidebarCategories> GetSidebarCategoriesAsync(string userId, string teamId, CancellationToken cancellationToken = default);

        /// <summary>Get a single sidebar category. Requires Mattermost 5.26 or later and access to the team.</summary>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="categoryId">Category identifier, including a built-in category identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<SidebarCategory> GetSidebarCategoryAsync(string userId, string teamId, string categoryId, CancellationToken cancellationToken = default);

        /// <summary>Get the ordered sidebar category identifiers. Requires Mattermost 5.26 or later.</summary>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<string>> GetSidebarCategoryOrderAsync(string userId, string teamId, CancellationToken cancellationToken = default);

        /// <summary>Create a custom sidebar category. Requires Mattermost 5.26 or later.</summary>
        /// <param name="category">Category with actual UserId, TeamId, DisplayName, and Type set to custom. The server assigns its identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<SidebarCategory> CreateSidebarCategoryAsync(SidebarCategory category, CancellationToken cancellationToken = default);

        /// <summary>Update a full sidebar category. Requires Mattermost 5.26 or later.</summary>
        /// <param name="category">Complete category with actual UserId, TeamId and Id. Read it first to preserve writable settings and channel order. To move channels between categories, update both categories with UpdateSidebarCategoriesAsync.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<SidebarCategory> UpdateSidebarCategoryAsync(SidebarCategory category, CancellationToken cancellationToken = default);

        /// <summary>Update selected full sidebar categories, including channel order. Requires Mattermost 5.26 or later.</summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="categories">Nonempty collection of categories belonging to this user and team, with unique identifiers. Unspecified categories are preserved; writable settings inside supplied categories are replaced. Moving a channel requires supplying both its source and destination categories.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<SidebarCategory>> UpdateSidebarCategoriesAsync(string userId, string teamId, IEnumerable<SidebarCategory> categories,
            CancellationToken cancellationToken = default);

        /// <summary>Set sidebar category order. Requires Mattermost 5.26 or later.</summary>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="categoryIds">Every category identifier exactly once, including built-in categories, in the desired order.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<string>> UpdateSidebarCategoryOrderAsync(string userId, string teamId, IEnumerable<string> categoryIds,
            CancellationToken cancellationToken = default);

        /// <summary>Delete a custom sidebar category. Built-in categories cannot be deleted. Requires Mattermost 5.26 or later.</summary>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="categoryId">Custom category identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteSidebarCategoryAsync(string userId, string teamId, string categoryId, CancellationToken cancellationToken = default);

        /// <summary>Autocomplete channels by name or display name within a team, respecting user visibility.</summary>
        /// <param name="teamId">Team identifier. Requires list_team_channels permission.</param>
        /// <param name="name">Name fragment. An empty string requests initial suggestions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Results may include archived channels and omit expensive channel properties.</remarks>
        Task<IList<Channel>> AutocompleteChannelsAsync(string teamId, string name, CancellationToken cancellationToken = default);

        /// <summary>Autocomplete the current user's channels for a team's search filters. Requires Mattermost 5.4 or later.</summary>
        /// <param name="teamId">Team context identifier.</param>
        /// <param name="name">Name or display-name fragment. An empty string requests initial suggestions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>May include archived channels and direct/group channels visible to the current user.</remarks>
        Task<IList<Channel>> AutocompleteChannelsForSearchAsync(string teamId, string name, CancellationToken cancellationToken = default);

        /// <summary>Get recommended public channels for the current user in a team.</summary>
        /// <param name="teamId">Team identifier. Requires list_team_channels permission.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Recommendations require Enterprise Advanced and enabled attribute-based access control. Returns an empty list when that feature is unavailable.</remarks>
        Task<IList<Channel>> GetRecommendedChannelsAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Get teams shared by all active members of a direct or group message channel. Requires Mattermost 9.1 or later.</summary>
        /// <param name="channelId">Direct or group message channel identifier. Requires read_channel permission; guests are not permitted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The common teams, or an empty list if none exist.</returns>
        Task<IList<Team>> GetChannelMembersCommonTeamsAsync(string channelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set call state for channel identifier - 'Calls' plugin required.
        /// </summary>
        /// <param name="isCallsEnabled"> New state. </param>
        /// <param name="channelId"> Channel identifier where calls must be in specified state. </param>
        Task SetChannelCallStateAsync(string channelId, bool isCallsEnabled);

        /// <summary>
        /// Check whether a call is active in the specified channel - 'Calls' plugin required.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> True when the channel has an active call; otherwise false. </returns>
        Task<bool> GetCallActiveAsync(string channelId);

        /// <summary>
        /// End the active call in the specified channel for all participants - 'Calls' plugin required.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        Task EndCallAsync(string channelId);
    }
}
