using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models;
using Mattermost.Models.Channels;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Posts, reactions, read state, reminders, synchronized drafts and followed threads.
    /// </summary>
    public interface IMattermostPostsClient
    {
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

        /// <summary>Get saved versions of a post authored by the current user.</summary>
        /// <param name="postId">Current post identifier, not a historical version identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Historical posts whose OriginalId refers to the current post.</returns>
        /// <remarks>Requires permission to edit the post. Server retention settings may limit the available history; no saved versions may produce HTTP 404.</remarks>
        Task<IList<Post>> GetPostEditHistoryAsync(string postId, CancellationToken cancellationToken = default);

        /// <summary>Get channel, team and membership information for a post without fetching its contents.</summary>
        /// <param name="postId">Post identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Location and membership information visible to the current user.</returns>
        /// <remarks>The result does not grant permission to read or join the channel.</remarks>
        Task<PostLocationInfo> GetPostInfoAsync(string postId, CancellationToken cancellationToken = default);

        /// <summary>Restore a saved version of a post authored by the current user.</summary>
        /// <param name="postId">Current post identifier.</param>
        /// <param name="restoreVersionId">Historical identifier returned by GetPostEditHistoryAsync.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The updated current post.</returns>
        /// <remarks>Requires permission to edit the post. Cancellation cannot undo an accepted restoration.</remarks>
        Task<Post> RestorePostVersionAsync(string postId, string restoreVersionId, CancellationToken cancellationToken = default);

        /// <summary>Mark a post and subsequent messages unread for a user.</summary>
        /// <param name="userId">User identifier or "me". Updating another user requires appropriate permissions.</param>
        /// <param name="postId">Post identifier.</param>
        /// <param name="collapsedThreadsSupported">Apply the server's collapsed-thread read-state handling.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The channel's updated read boundary and mention counts, not its total unread message count.</returns>
        Task<ChannelReadState> MarkPostUnreadAsync(string userId, string postId, bool collapsedThreadsSupported = false,
            CancellationToken cancellationToken = default);

        /// <summary>Schedule or replace a user's reminder for a post.</summary>
        /// <param name="userId">User identifier or "me". Updating another user requires appropriate permissions.</param>
        /// <param name="postId">Post identifier.</param>
        /// <param name="remindAt">Reminder time, sent as Unix seconds with subsecond precision discarded.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires permission to read the post. Cancellation cannot undo an accepted reminder.</remarks>
        Task SetPostReminderAsync(string userId, string postId, DateTimeOffset remindAt, CancellationToken cancellationToken = default);

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

        /// <summary>Get the authenticated user's server-synchronized drafts for a team.</summary>
        /// <param name="teamId">Team identifier. Requires view_team permission.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires AllowSyncedDrafts. The server returns HTTP 501 when synced drafts are disabled.</remarks>
        /// <returns>Drafts, or an empty list when the server returns null or an empty array.</returns>
        Task<IList<Draft>> GetDraftsAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Create or replace the authenticated user's channel or thread draft.</summary>
        /// <param name="draft">Complete draft. ChannelId and Text are required. Empty Text deletes the draft. RootId identifies the thread root, or is empty for a channel draft.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Saved draft, or null when an empty message deletes it.</returns>
        /// <remarks>Requires AllowSyncedDrafts and permission to create posts in the channel. UserId, timestamps and Metadata are server-owned and are not sent. This is not a patch: omitted properties are not preserved.</remarks>
        Task<Draft?> UpsertDraftAsync(Draft draft, CancellationToken cancellationToken = default);

        /// <summary>Delete the authenticated user's synced draft. Deleting an absent draft succeeds.</summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="threadId">Root post identifier for a reply draft. Null deletes the channel draft instead.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires AllowSyncedDrafts. Does not delete posted messages.</remarks>
        Task DeleteDraftAsync(string channelId, string? threadId = null, CancellationToken cancellationToken = default);

        /// <summary>Get followed threads and aggregate unread counts. Requires Mattermost 5.29 or later and access to the team.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier. Direct and group messages are included unless excludeDirect is true.</param>
        /// <param name="perPage">Positive chunk size. Pagination uses thread cursors, not page numbers.</param>
        /// <param name="beforeThreadId">Return threads before this cursor. Cannot be combined with afterThreadId.</param>
        /// <param name="afterThreadId">Return threads after this cursor. Cannot be combined with beforeThreadId.</param>
        /// <param name="since">Filter memberships updated since this nonnegative Unix millisecond timestamp.</param>
        /// <param name="unread">Only return threads with unread replies.</param>
        /// <param name="extended">Include participant profile details.</param>
        /// <param name="deleted">Include deleted threads for synchronization.</param>
        /// <param name="totalsOnly">Return counts without threads. Cannot be combined with threadsOnly.</param>
        /// <param name="threadsOnly">Return threads without computing counts; totals are zero.</param>
        /// <param name="excludeDirect">Exclude direct and group message threads.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<UserThreadsResponse> GetUserThreadsAsync(string userId, string teamId, int perPage = 60,
            string? beforeThreadId = null, string? afterThreadId = null, long since = 0, bool unread = false,
            bool extended = false, bool deleted = false, bool totalsOnly = false, bool threadsOnly = false,
            bool excludeDirect = false, CancellationToken cancellationToken = default);

        /// <summary>Get a followed thread, its root post and unread state. Requires Mattermost 5.29 or later.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="threadId">Root post identifier, not a reply identifier.</param>
        /// <param name="extended">Include participant profile details.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<UserThread> GetUserThreadAsync(string userId, string teamId, string threadId, bool extended = false,
            CancellationToken cancellationToken = default);

        /// <summary>Follow a thread. Requires Mattermost 5.29 or later and permission to read the root post.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="threadId">Root post identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task FollowThreadAsync(string userId, string teamId, string threadId, CancellationToken cancellationToken = default);

        /// <summary>Stop following a thread. Does not delete posts. Requires Mattermost 5.29 or later.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="threadId">Root post identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UnfollowThreadAsync(string userId, string teamId, string threadId, CancellationToken cancellationToken = default);

        /// <summary>Set a followed thread's read timestamp. Requires Mattermost 5.29 or later.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="threadId">Root post identifier.</param>
        /// <param name="timestamp">Positive Unix millisecond timestamp.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Updated thread and unread state.</returns>
        Task<UserThread> UpdateThreadReadAsync(string userId, string teamId, string threadId, long timestamp,
            CancellationToken cancellationToken = default);

        /// <summary>Mark a thread unread starting at a post, and follow the thread. Requires Mattermost 6.7 or later.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="threadId">Root post identifier.</param>
        /// <param name="postId">Post in this thread from which to mark unread.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Updated thread and unread state.</returns>
        Task<UserThread> MarkThreadUnreadAsync(string userId, string teamId, string threadId, string postId,
            CancellationToken cancellationToken = default);

        /// <summary>Mark all followed threads for a team as read. Requires Mattermost 5.29 or later and access to the team.</summary>
        /// <param name="userId">User identifier or me. Access to another user requires edit_other_users.</param>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task MarkAllThreadsReadAsync(string userId, string teamId, CancellationToken cancellationToken = default);
    }
}
