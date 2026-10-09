using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Models;
using Mattermost.Models.Channels;
using Mattermost.Models.Dialogs;
using Mattermost.Models.Emojis;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using Mattermost.Models.Roles;
using Mattermost.Models.Teams;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Mattermost client interface.
    /// </summary>
    public interface IMattermostClient
    {
        /// <summary>
        /// Specifies whether the client is connected to the server with WebSocket.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// User information.
        /// </summary>
        User CurrentUserInfo { get; }

        /// <summary>
        /// Base server address.
        /// </summary>
        Uri ServerAddress { get; }

        /// <summary>
        /// Occurs when the WebSocket connection is successfully established.
        /// </summary>
        event EventHandler<ConnectionEventArgs>? OnConnected;

        /// <summary>
        /// Occurs when the WebSocket is disconnected, either by the client or the server.
        /// </summary>
        event EventHandler<DisconnectionEventArgs>? OnDisconnected;

        /// <summary>
        /// Event called in independent thread when new message received.
        /// </summary>
        event EventHandler<MessageEventArgs>? OnMessageReceived;

        /// <summary>
        /// Event callen in independent thread when log message created.
        /// </summary>
        event EventHandler<LogEventArgs>? OnLogMessage;

        /// <summary>
        /// Create receiver <see cref="Task"/> with websocket polling.
        /// </summary>
        /// <returns> Receiver task. </returns>
        Task StartReceivingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop receiving messages.
        /// </summary>
        Task StopReceivingAsync();

        #region Posts

        /// <summary>Search posts in a team visible to the current user. Requires view_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="terms">Nonblank search terms, including Mattermost operators such as from: and in:.</param>
        /// <param name="isOrSearch">Use OR instead of AND between search terms.</param>
        /// <param name="page">Zero-based page. Pagination depends on the server's search backend.</param>
        /// <param name="perPage">Positive page size. Pagination depends on the server's search backend.</param>
        /// <param name="timeZoneOffset">Offset from UTC in seconds for date searches.</param>
        /// <param name="includeDeletedChannels">Include archived channels in the search.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Post identifiers, posts, and matched terms when provided by the search backend.</returns>
        Task<PostSearchResponse> SearchTeamPostsAsync(string teamId, string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default);

        /// <summary>Search posts visible to the current user across all teams.</summary>
        /// <param name="terms">Nonblank Mattermost search terms.</param>
        /// <param name="isOrSearch">Use OR instead of AND between search terms.</param>
        /// <param name="page">Zero-based page. Pagination depends on the server's search backend.</param>
        /// <param name="perPage">Positive page size. Pagination depends on the server's search backend.</param>
        /// <param name="timeZoneOffset">Offset from UTC in seconds for date searches.</param>
        /// <param name="includeDeletedChannels">Include archived channels in the search.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Post identifiers, posts, and matched terms when provided by the search backend.</returns>
        Task<PostSearchResponse> SearchPostsAsync(string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default);

        /// <summary>Get a user's saved posts.</summary>
        /// <param name="userId">User identifier or me. Another user's saved posts require edit_other_users permission.</param>
        /// <param name="teamId">Optional team filter.</param>
        /// <param name="channelId">Optional channel filter. Takes precedence over the team filter when both are supplied.</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Saved post identifiers and visible posts.</returns>
        Task<ChannelPostsResponse> GetFlaggedPostsAsync(string userId, string? teamId = null,
            string? channelId = null, int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get posts around a user's oldest unread post in a channel. Requires Mattermost 5.14 or later.</summary>
        /// <param name="userId">User identifier or me. Another user's unread state requires edit_other_users permission.</param>
        /// <param name="channelId">Channel identifier. Requires permission to read the channel.</param>
        /// <param name="limitBefore">Posts before the unread boundary, from zero to 200.</param>
        /// <param name="limitAfter">Posts at and after the unread boundary, from one to 200.</param>
        /// <param name="skipFetchThreads">Skip fetching thread replies.</param>
        /// <param name="collapsedThreads">Use collapsed reply threads.</param>
        /// <param name="collapsedThreadsExtended">Include thread participant details.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Posts around the unread boundary, or recent posts when none are unread. Reading does not mark them read.</returns>
        Task<ChannelPostsResponse> GetUnreadPostsAsync(string userId, string channelId,
            int limitBefore = 60, int limitAfter = 60, bool skipFetchThreads = false, bool collapsedThreads = false,
            bool collapsedThreadsExtended = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message to specified channel using channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="rawProps"> A general JSON property bag to attach to the post. </param>
        /// <param name="requestedAck"> Request acknowledgement from recipients. </param>
        /// <param name="persistentNotifications"> Send persistent notifications until acknowledgement. Urgent posts only. </param>
        /// <returns> Created post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        Task<Post> CreatePostWithRawPropsAsync(string channelId, string message = "", string replyToPostId = "",
            MessagePriority priority = MessagePriority.Empty, IEnumerable<string>? files = null,
            IDictionary<string, object>? rawProps = null, bool requestedAck = false,
            bool persistentNotifications = false);

        /// <summary>
        /// Send message to specified channel using channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="props"> Props object to attach to the post. </param>
        /// <param name="requestedAck"> Request acknowledgement from recipients. </param>
        /// <param name="persistentNotifications"> Send persistent notifications until acknowledgement. Urgent posts only. </param>
        /// <returns> Created post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        Task<Post> CreatePostAsync(string channelId, string message = "", string replyToPostId = "",
            MessagePriority priority = MessagePriority.Empty, IEnumerable<string>? files = null,
            PostProps? props = null, bool requestedAck = false, bool persistentNotifications = false);

        /// <summary>
        /// Get post by identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Post information. </returns>
        Task<Post> GetPostAsync(string postId);

        /// <summary>
        /// Get multiple posts in one request.
        /// </summary>
        /// <param name="postIds">Between one and <see cref="MattermostApiLimits.MaxPostIdsPerRequest"/> post identifiers.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Posts visible to the current user. Missing or inaccessible posts may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty, exceeds the limit, or contains blank identifiers.</exception>
        Task<IList<Post>> GetPostsByIdsAsync(IEnumerable<string> postIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a channel's pinned posts.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Pinned post identifiers and their corresponding posts.</returns>
        /// <exception cref="ArgumentException">The channel identifier is null, empty, or whitespace.</exception>
        Task<ChannelPostsResponse> GetPinnedPostsAsync(string channelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get metadata for the files attached to a post, without downloading their contents.
        /// </summary>
        /// <param name="postId">Post identifier.</param>
        /// <param name="includeDeleted">Include deleted post data. Requires system administrator permissions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Metadata for the attached files.</returns>
        /// <exception cref="ArgumentException">The post identifier is null, empty, or whitespace.</exception>
        Task<IList<FileDetails>> GetPostFilesAsync(string postId, bool includeDeleted = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add current user's reaction to a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="emojiName"> Emoji name without surrounding colons. </param>
        /// <returns> Created reaction information. </returns>
        Task<Reaction> AddReactionAsync(string postId, string emojiName);

        /// <summary>
        /// Remove a reaction from a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="emojiName"> Emoji name without surrounding colons. </param>
        /// <param name="userId"> User identifier. Defaults to current user. </param>
        Task RemoveReactionAsync(string postId, string emojiName, string? userId = null);

        /// <summary>
        /// Get reactions for a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Reactions for the post. </returns>
        Task<IList<Reaction>> GetReactionsAsync(string postId);

        /// <summary>
        /// Pin a post to its channel.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        Task PinPostAsync(string postId);

        /// <summary>
        /// Unpin a post from its channel.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        Task UnpinPostAsync(string postId);

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="rawProps"> A general JSON property bag to attach to the post. </param>
        /// <returns> Updated post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        Task<Post> UpdatePostWithRawPropsAsync(string postId, string newText, IDictionary<string, object>? rawProps = null);

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="props"> Props object to attach to the post. </param>
        /// <returns> Updated post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        Task<Post> UpdatePostAsync(string postId, string newText, PostProps? props = null);

        /// <summary>
        /// Partially update a post. Null parameters are omitted, leaving the corresponding fields unchanged.
        /// </summary>
        /// <param name="postId">Post identifier.</param>
        /// <param name="text">Replacement message text (Markdown supported). An empty string sends empty text.</param>
        /// <param name="props">Replacement JSON property bag, not a per-key merge. An empty dictionary sends empty props.</param>
        /// <param name="fileIds">Replacement attached file identifiers. An empty list removes attachments.</param>
        /// <param name="isPinned">Whether the post is pinned. False removes the pin.</param>
        /// <param name="hasReactions">Replacement reaction flag. This does not add or remove individual reactions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Updated post.</returns>
        /// <remarks>Requires edit_post permission for the author's post, or edit_others_posts for another user's post. Server validation and editing restrictions apply.</remarks>
        /// <exception cref="ArgumentException">The post identifier is null, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The supplied text exceeds <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        Task<Post> PatchPostAsync(string postId, string? text = null, IDictionary<string, object>? props = null,
            IList<string>? fileIds = null, bool? isPinned = null, bool? hasReactions = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete post with specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        Task DeletePostAsync(string postId);

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of posts per page. </param>
        /// <param name="beforePostId"> A post id to select the posts that came before this one. </param>
        /// <param name="afterPostId"> A post id to select the posts that came after this one. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <param name="since"> Time to select modified posts after. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        public Task<ChannelPostsResponse> GetChannelPostsAsync(string channelId, int page = 0,
            int perPage = 60, string? beforePostId = null, string? afterPostId = null,
            bool includeDeleted = false, DateTime? since = null);

        /// <summary>
        /// Get posts related to specified post identifier in thread format.
        /// </summary>
        /// <param name="postId"> Post identifier to get thread posts. </param>
        /// <param name="fromPostId"> Post identifier to start from. </param>
        /// <returns> Collection of posts in thread format. </returns>
        public Task<ChannelPostsResponse> GetThreadPostsAsync(string postId, string? fromPostId = null);

        #endregion

        #region Interactive Dialogs

        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="triggerId"> Trigger identifier from a slash command or interactive action payload. </param>
        /// <param name="url"> URL where Mattermost sends the submitted dialog payload. </param>
        /// <param name="dialog"> Dialog definition. </param>
        Task OpenInteractiveDialogAsync(string triggerId, string url, InteractiveDialog dialog);

        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="request"> Open dialog request. </param>
        Task OpenInteractiveDialogAsync(OpenInteractiveDialogRequest request);

        #endregion

        #region Teams

        /// <summary>Create a team. Requires create_team permission.</summary>
        /// <param name="name">Team URL name. Surrounding whitespace is removed.</param>
        /// <param name="displayName">Team display name. Surrounding whitespace is removed.</param>
        /// <param name="type">Open or invite-only membership.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The created team.</returns>
        Task<Team> CreateTeamAsync(string name, string displayName, TeamType type,
            CancellationToken cancellationToken = default);

        /// <summary>Partially update team metadata. Requires manage_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="displayName">Replacement display name.</param>
        /// <param name="description">Replacement description. An empty string clears it.</param>
        /// <param name="companyName">Replacement company name. An empty string clears it.</param>
        /// <param name="allowedDomains">Replacement allowed email domains. An empty string removes the restriction.</param>
        /// <param name="allowOpenInvite">Whether open invitations are allowed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The updated team.</returns>
        /// <remarks>Null parameters are omitted. Empty strings and false are sent unchanged. Server validation applies.</remarks>
        Task<Team> PatchTeamAsync(string teamId, string? displayName = null, string? description = null,
            string? companyName = null, string? allowedDomains = null, bool? allowOpenInvite = null,
            CancellationToken cancellationToken = default);

        /// <summary>Search teams visible to the current user, without pagination.</summary>
        /// <param name="term">Search term. An empty string matches all visible teams.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching teams. Visibility depends on team-listing permissions.</returns>
        Task<IList<Team>> SearchTeamsAsync(string term, CancellationToken cancellationToken = default);

        /// <summary>Add a user to a team.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">Actual user identifier; the me alias is not supported.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The team membership.</returns>
        /// <remarks>Adding another user requires add_user_to_team permission. Joining as the current user requires an open team or appropriate membership permission.</remarks>
        Task<TeamMember> AddTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get team by specified identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <returns> Team information. </returns>
        Task<Team> GetTeamAsync(string teamId);

        /// <summary>
        /// Get a page of teams.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of teams per page. </param>
        /// <returns> Teams visible to the current user. </returns>
        Task<IList<Team>> GetTeamsAsync(int page = 0, int perPage = 60);

        /// <summary>
        /// Get teams for a specified user.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Teams the user belongs to. </returns>
        Task<IReadOnlyList<Team>> GetUserTeamsAsync(string userId);

        /// <summary>
        /// Get team by name.
        /// </summary>
        /// <param name="teamName"> Team name. </param>
        /// <returns> Team information. </returns>
        Task<Team> GetTeamByNameAsync(string teamName);

        /// <summary>
        /// Get one page of team memberships. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Members per page, from 1 to <see cref="MattermostApiLimits.MaxTeamMembersPerPage"/>.</param>
        /// <param name="sortByUsername">Sort by username instead of user identifier.</param>
        /// <param name="excludeDeletedUsers">Exclude deactivated users.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Membership records, not user profiles. The server may hide other members' role data.</returns>
        /// <exception cref="ArgumentException">The team identifier is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The page is negative or the page size is outside the allowed range.</exception>
        Task<IList<TeamMember>> GetTeamMembersAsync(string teamId, int page = 0, int perPage = 60,
            bool sortByUsername = false, bool excludeDeletedUsers = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's team membership. Requires permission to view the team and user.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Membership record. A missing membership is reported as an API error.</returns>
        /// <exception cref="ArgumentException">An identifier is blank.</exception>
        Task<TeamMember> GetTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get team memberships by user identifiers in one request. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userIds">A nonempty collection of actual user identifiers, not "me".</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible membership records. Missing or inaccessible members may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The team identifier is blank, or the collection is empty or contains blank identifiers.</exception>
        Task<IList<TeamMember>> GetTeamMembersByIdsAsync(string teamId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's memberships across teams, subject to server visibility permissions.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Team membership and role records. The server may hide role data.</returns>
        /// <exception cref="ArgumentException">The user identifier is blank.</exception>
        Task<IList<TeamMember>> GetUserTeamMembersAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get membership counts for a team. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Total and active member counts, subject to visibility restrictions.</returns>
        /// <exception cref="ArgumentException">The team identifier is blank.</exception>
        Task<TeamStats> GetTeamStatsAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check whether a team exists and is visible to the current user.
        /// </summary>
        /// <param name="teamName">Team URL name, not its display name.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>True when a visible team exists; false otherwise. HTTP failures are propagated.</returns>
        /// <exception cref="ArgumentException">The team name is blank.</exception>
        Task<bool> TeamExistsAsync(string teamName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's unread counts for a team. Requires permission to access the user and view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Unread message and mention counts. This does not mark messages as read.</returns>
        /// <exception cref="ArgumentException">An identifier is blank.</exception>
        Task<TeamUnread> GetTeamUnreadAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unread counts across a user's teams. Reading another user's counts requires system administrator permissions.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="excludeTeamId">Team to omit. Null, empty or whitespace means no exclusion.</param>
        /// <param name="includeCollapsedThreads">Include unread counts for followed collapsed threads.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Unread counts for the user's teams. This does not mark messages as read.</returns>
        /// <exception cref="ArgumentException">The user identifier is blank.</exception>
        Task<IList<TeamUnread>> GetUserTeamsUnreadAsync(string userId, string? excludeTeamId = null,
            bool includeCollapsedThreads = false, CancellationToken cancellationToken = default);

        #endregion

        #region Channels

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

        #endregion

        #region Files

        /// <summary>Download a file's preview image.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete preview image as bytes.</returns>
        /// <remarks>Requires channel-read permission or file ownership. Missing previews produce a server error; the original file is not substituted.</remarks>
        Task<byte[]> GetFilePreviewAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Download a file's thumbnail image.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete thumbnail image as bytes.</returns>
        /// <remarks>Requires channel-read permission or file ownership. Missing thumbnails produce a server error.</remarks>
        Task<byte[]> GetFileThumbnailAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Get a file link that can be opened without authentication.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The public file URL.</returns>
        /// <remarks>Requires channel-read permission or file ownership, and public file links enabled on the server. Anyone with the returned link can access the file.</remarks>
        Task<string> GetPublicFileLinkAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get file by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File bytes. </returns>
        Task<byte[]> GetFileAsync(string fileId);

        /// <summary>
        /// Get file stream by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File stream. </returns>
        Task<Stream> GetFileStreamAsync(string fileId);

        /// <summary>
        /// Get file details by specified identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File details. </returns>
        Task<FileDetails> GetFileDetailsAsync(string fileId);

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="filePath"> File fullname on local device. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        Task<FileDetails> UploadFileAsync(string channelId, string filePath, Action<int>? progressChanged = null);

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="fileName"> Name of the uploaded file. </param>
        /// <param name="stream"> File content. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        Task<FileDetails> UploadFileAsync(string channelId, string fileName, Stream stream, Action<int>? progressChanged = null);

        #endregion

        #region Users

        /// <summary>Download a user's current profile image.</summary>
        /// <param name="userId">User identifier or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete profile image as bytes.</returns>
        /// <remarks>Requires authentication and permission to view the user. The server returns its generated image when no custom image is set.</remarks>
        Task<byte[]> GetUserImageAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Download a user's generated default profile image, even when a custom image is set. Requires Mattermost 5.5 or later.</summary>
        /// <param name="userId">User identifier or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete default image as bytes.</returns>
        /// <remarks>Requires authentication and permission to view the user.</remarks>
        Task<byte[]> GetDefaultUserImageAsync(string userId, CancellationToken cancellationToken = default);

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

        /// <summary>
        /// Get current authorized user information.
        /// </summary>
        /// <returns> Authorized user information. </returns>
        Task<User> GetMeAsync();

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Get a page of users.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of users per page. </param>
        /// <param name="inTeamId"> Only users in this team. </param>
        /// <param name="notInTeamId"> Only users not in this team. </param>
        /// <param name="inChannelId"> Only users in this channel. </param>
        /// <param name="notInChannelId"> Only users not in this channel. </param>
        /// <param name="active"> Only active users. </param>
        /// <param name="inactive"> Only inactive users. </param>
        /// <returns> Users matching the query. </returns>
        Task<IList<User>> GetUsersAsync(
            int page = 0,
            int perPage = 60,
            string? inTeamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool? active = null,
            bool? inactive = null);

        /// <summary>
        /// Search users by term.
        /// </summary>
        /// <param name="term"> Search term matched against username, full name, nickname and email. </param>
        /// <param name="teamId"> Only search users on this team. </param>
        /// <param name="notInTeamId"> Only search users not on this team. </param>
        /// <param name="inChannelId"> Only search users in this channel. </param>
        /// <param name="notInChannelId"> Only search users not in this channel. Must specify teamId when using this option. </param>
        /// <param name="groupConstrained"> Only users allowed to join based on group constraints. </param>
        /// <param name="allowInactive"> Include deactivated users in the results. </param>
        /// <param name="withoutTeam"> Search users that are not on a team. </param>
        /// <param name="limit"> Maximum number of users to return. </param>
        /// <returns> Users matching the search term. </returns>
        Task<IList<User>> SearchUsersAsync(
            string term,
            string? teamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool groupConstrained = false,
            bool allowInactive = false,
            bool withoutTeam = false,
            int? limit = null);

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email"> Email address. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>
        /// Get multiple user profiles in one request.
        /// </summary>
        /// <param name="userIds">A nonempty collection of user identifiers. Surrounding whitespace is removed.</param>
        /// <param name="since">Only profiles updated after this Unix timestamp in milliseconds. Zero disables filtering. Requires Mattermost 5.14 or later when used.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible user profiles. Missing or inaccessible users may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank identifiers.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The timestamp is negative.</exception>
        Task<IList<User>> GetUsersByIdsAsync(IEnumerable<string> userIds, long since = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get multiple user profiles by username in one request.
        /// </summary>
        /// <param name="usernames">A nonempty collection of usernames. Surrounding whitespace and leading @ characters are removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible user profiles. Missing or inaccessible users may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank usernames.</exception>
        Task<IList<User>> GetUsersByUsernamesAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's presence status.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Presence status and activity metadata.</returns>
        /// <exception cref="ArgumentException">The user identifier is null, empty, or whitespace.</exception>
        Task<UserPresence> GetUserStatusAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get multiple users' presence statuses in one request.
        /// </summary>
        /// <param name="userIds">A nonempty collection of actual user identifiers, not "me". Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Presence statuses returned by the server; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank identifiers.</exception>
        Task<IList<UserPresence>> GetUsersStatusesByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

        #endregion

        #region Channel management and discovery

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

        #endregion

        #region Preferences

        /// <summary>Get a user's stored preferences. Requires access to that user.</summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Preference>> GetPreferencesAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Get preferences in one category. A missing category produces an API error.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="category">Exact category key, without trimming. Server route restrictions apply.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Preference>> GetPreferencesByCategoryAsync(string userId, string category, CancellationToken cancellationToken = default);

        /// <summary>Get a preference by category and name. A missing preference produces an API error.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="category">Exact category key, without trimming. Server route restrictions apply.</param>
        /// <param name="name">Exact nonempty preference key, without trimming. Use category lookup for preferences with empty names.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Preference> GetPreferenceAsync(string userId, string category, string name, CancellationToken cancellationToken = default);

        /// <summary>Save 1–100 preferences. Unspecified preferences are not changed.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="preferences">Preferences with the target user's actual UserId. Names and values may be empty and are not trimmed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdatePreferencesAsync(string userId, IEnumerable<Preference> preferences, CancellationToken cancellationToken = default);

        /// <summary>Delete 1–100 preferences by user, category, and name.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="preferences">Preference keys with the target user's actual UserId. Value is ignored by the server.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeletePreferencesAsync(string userId, IEnumerable<Preference> preferences, CancellationToken cancellationToken = default);

        #endregion

        #region Roles

        /// <summary>Get all roles. Requires Mattermost 5.33 or later and manage_system permission.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Role>> GetRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>Get a role by identifier. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleId">Role identifier; surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Role> GetRoleAsync(string roleId, CancellationToken cancellationToken = default);

        /// <summary>Get a role by name. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleName">Role name; surrounding whitespace is removed and case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Role> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>Get roles by name. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleNames">Nonempty collection of up to 100 distinct names; whitespace and duplicate names are removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching roles. Input order is not preserved; missing names may be omitted.</returns>
        Task<IList<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

        #endregion

        #region Emojis

        /// <summary>
        /// Get a page of custom emoji metadata. Requires authentication and enabled custom emojis.
        /// </summary>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Items per page, from 1 to 200.</param>
        /// <param name="sortByName">Sort by emoji name. Requires Mattermost 4.7 or later when enabled.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emojis on the requested page. Built-in emojis are not included.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Page or page size is outside the allowed range.</exception>
        Task<IList<Emoji>> GetEmojisAsync(int page = 0, int perPage = 60, bool sortByName = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata by identifier. Requires authentication and enabled custom emojis.
        /// </summary>
        /// <param name="emojiId">Emoji identifier. Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emoji metadata. A missing emoji produces an API error.</returns>
        /// <exception cref="ArgumentException">The identifier is null, empty, or whitespace.</exception>
        Task<Emoji> GetEmojiAsync(string emojiId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata by name. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="emojiName">Emoji name. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emoji metadata. A missing emoji produces an API error.</returns>
        /// <exception cref="ArgumentException">The name is null or blank after trimming.</exception>
        Task<Emoji> GetEmojiByNameAsync(string emojiName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata for multiple names. Requires Mattermost 9.2 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="emojiNames">Nonempty collection containing up to 200 distinct names. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching custom emojis. Missing and built-in names are omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty, contains blank names, or exceeds 200 distinct names.</exception>
        Task<IList<Emoji>> GetEmojisByNamesAsync(IEnumerable<string> emojiNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search custom emoji names. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="term">Nonempty search term. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="prefixOnly">Match only names starting with the term instead of names containing it.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Up to 200 matching custom emojis, sorted by name.</returns>
        /// <exception cref="ArgumentException">The term is null or blank after trimming.</exception>
        Task<IList<Emoji>> SearchEmojisAsync(string term, bool prefixOnly = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find custom emojis whose names start with a prefix. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="name">Nonempty name prefix. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Up to 100 matching custom emojis, sorted by name.</returns>
        /// <exception cref="ArgumentException">The prefix is null or blank after trimming.</exception>
        Task<IList<Emoji>> AutocompleteEmojisAsync(string name, CancellationToken cancellationToken = default);

        #endregion

        #region Calls

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

        #endregion

        /// <summary>
        /// Login with specified login identifier and password.
        /// </summary>
        /// <param name="username">Username or email.</param>
        /// <param name="password">Password.</param>
        /// <returns>Authorized <see cref="User"/> object.</returns>
        Task<User> LoginAsync(string username, string password);

        /// <summary>
        /// Logout from server.
        /// </summary>
        /// <returns> Task representing logout operation. </returns>
        Task LogoutAsync();
    }
}
