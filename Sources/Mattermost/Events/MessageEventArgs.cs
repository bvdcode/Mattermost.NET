using System;
using System.Threading;
using Mattermost.Models.Responses;
using Mattermost.Models.Responses.Websocket;

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
        public IMattermostClient Client { get; set; } = null!;

        /// <summary>
        /// Cancellation token from <see cref="IMattermostClient.StartReceivingAsync(CancellationToken)"/>
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Received message.
        /// </summary>
        public PostUpdateInfo Message { get; } = null!;

        internal MessageEventArgs(IMattermostClient mattermostBot, WebsocketMessage response, CancellationToken cancellationToken)
        {
            Client = mattermostBot;
            CancellationToken = cancellationToken;
            Message = response.GetData<PostInfo>().ToPostUpdateInfo();
        }
    }
}