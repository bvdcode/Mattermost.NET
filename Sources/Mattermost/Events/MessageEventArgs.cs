using System;
using System.Threading;
using Mattermost.Models.Responses.Websocket;
using Mattermost.Models.Responses.Websocket.Posts;

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

        internal MessageEventArgs(IMattermostClient mattermostBot, WebsocketMessage response, CancellationToken cancellationToken)
        {
            Client = mattermostBot;
            CancellationToken = cancellationToken;
            Message = response.GetData<PostInfo>();
        }
    }
}