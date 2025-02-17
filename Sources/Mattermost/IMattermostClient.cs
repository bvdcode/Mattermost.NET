using System;
using System.Threading;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Models;
using System.Threading.Tasks;
using Mattermost.Models.Users;
using Mattermost.Models.Posts;
using Mattermost.Models.Channels;
using System.Collections.Generic;
using System.IO;

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
        Task StartReceivingAsync();

        /// <summary>
        /// Create receiver <see cref="Task"/> with websocket polling.
        /// </summary>
        /// <returns> Receiver task. </returns>
        Task StartReceivingAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop receiving messages.
        /// </summary>
        Task StopReceivingAsync();

        #region Posts

        /// <summary>
        /// Send message to specified channel using channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="props"> A general JSON property bag to attach to the post. </param>
        /// <returns> Created post. </returns>
        Task<Post> CreatePostAsync(string channelId, string message = "", string replyToPostId = "", MessagePriority priority = MessagePriority.Empty, IEnumerable<string>? files = null, IDictionary<string, object>? props = null);

        /// <summary>
        /// Get post by identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Post information. </returns>
        Task<Post> GetPostAsync(string postId);

        /// <summary>
        /// Send message to specified channel using channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <returns> Created post. </returns>
        [Obsolete("Use CreatePostAsync instead.")]
        Task<Post> SendMessageAsync(string channelId, string message = "", string replyToPostId = "", MessagePriority priority = MessagePriority.Empty, IEnumerable<string>? files = null);

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="props"> A general JSON property bag to attach to the post. </param>
        /// <returns> Updated post. </returns>
        Task<Post> UpdatePostAsync(string postId, string newText, IDictionary<string, object>? props = null);

        /// <summary>
        /// Delete post with specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        Task<bool> DeletePostAsync(string postId);

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of posts per page. </param>
        /// <param name="beforePostId"> A post id to select the posts that came before this one. </param>
        /// <param name="afterPostId"> A post id to select the posts that came after this one. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        Task<ChannelPosts> GetPostsForChannelAsync(string channelId, int page = 0, int perPage = 60, string? beforePostId = null, string? afterPostId = null, bool includeDeleted = false);

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="since"> Time to select modified posts after. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        Task<ChannelPosts> GetPostsForChannelAsync(string channelId, DateTime since, bool includeDeleted = false);

        #endregion

        #region Channels

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
        Task<bool> DeleteUserFromChannelAsync(string channelId, string userId);

        /// <summary>
        /// Find channel by channel name and team identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier where channel is exists. </param>
        /// <param name="channelName"> Channel name. </param>
        /// <returns> Channel info. </returns>
        Task<Channel?> FindChannelByNameAsync(string teamId, string channelName);

        /// <summary>
        /// Archive channel by specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> True if archieved, otherwise false. </returns>
        Task<bool> ArchiveChannelAsync(string channelId);

        #endregion

        #region Files

        /// <summary>
        /// Get file by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File bytes. </returns>
        Task<byte[]> GetFileAsync(string fileId);

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
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserByUsernameAsync(string username);

        #endregion

        /// <summary>
        /// Set call state for channel identifier - 'Calls' plugin required.
        /// </summary>
        /// <param name="isCallsEnabled"> New state. </param>
        /// <param name="channelId"> Channel identifier where calls must be in specified state. </param>
        /// <returns> True if calls state setted, otherwise false. </returns>
        Task<bool> SetChannelCallStateAsync(string channelId, bool isCallsEnabled);
        
        /// <summary>
        /// Login with specified login identifier and password.
        /// </summary>
        /// <param name="loginId">Username or email.</param>
        /// <param name="password">Password.</param>
        /// <returns>Authorized <see cref="User"/> object.</returns>
        Task<User> LoginAsync(string loginId, string password);

        /// <summary>
        /// Logout from server.
        /// </summary>
        /// <returns> Task representing logout operation. </returns>
        Task LogoutAsync();
    }
}