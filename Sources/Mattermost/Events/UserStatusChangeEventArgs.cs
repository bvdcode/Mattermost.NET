using System;
using System.Threading;
using Mattermost.Models.Responses.Websocket;
using Mattermost.Models.Responses.Websocket.Users;

namespace Mattermost.Events
{
    /// <summary>
    /// User status change event data.
    /// </summary>
    public class UserStatusChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Mattermost client instance.
        /// </summary>
        public IMattermostClient Client { get; } = null!;

        /// <summary>
        /// Cancellation token from <see cref="IMattermostClient.StartReceivingAsync(CancellationToken)"/>
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// User status update information.
        /// </summary>
        public UserStatusUpdate UserStatusUpdate { get; } = null!;

        /// <summary>
        /// Specifies whether the current authorized user is the user whose status has changed.
        /// </summary>
        public bool IsCurrentUser => Client.CurrentUserInfo.Id == UserStatusUpdate.UserId;

        internal UserStatusChangeEventArgs(MattermostClient mattermostClient, WebsocketMessage response, CancellationToken cancellationToken)
        {
            Client = mattermostClient;
            CancellationToken = cancellationToken;
            UserStatusUpdate = response.GetData<UserStatusUpdate>();
        }
    }
}