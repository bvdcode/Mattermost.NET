using Mattermost.Models.Responses.Websocket;
using Mattermost.Models.Responses.Websocket.Posts;
using System;
using System.Threading;

namespace Mattermost.Events
{
    /// <summary>
    /// Update event message data.
    /// </summary>
    public class MessageEventArgs : EventArgs
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
        /// Received message.
        /// </summary>
        public PostInfo Message { get; } = null!;

        /// <summary>
        /// Specifies whether the current authorized user is the message author.
        /// </summary>
        public bool IsCurrentUser { get; }

        internal MessageEventArgs(IMattermostClient mattermostBot, WebsocketMessage response, CancellationToken cancellationToken, string? currentUserId = null)
        {
            Client = mattermostBot;
            CancellationToken = cancellationToken;
            Message = response.GetData<PostInfo>();
            IsCurrentUser = !string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(Message.Post.UserId, currentUserId, StringComparison.Ordinal);
        }
    }
}
