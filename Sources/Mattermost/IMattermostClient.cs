using Mattermost.Events;
using Mattermost.Models.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Mattermost client interface.
    /// </summary>
    public interface IMattermostClient :
        IMattermostGroupsClient,
        IMattermostUsersClient,
        IMattermostPostsClient,
        IMattermostChannelsClient,
        IMattermostTeamsClient,
        IMattermostFilesClient,
        IMattermostIntegrationsClient,
        IMattermostEmojisClient,
        IMattermostSystemClient
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
