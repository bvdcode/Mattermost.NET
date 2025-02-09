using System.Threading;
using Mattermost.Enums;
using Mattermost.Models.Responses.Websocket;

namespace Mattermost.Events
{
    /// <summary>
    /// WebSocket event message data.
    /// </summary>
    public class WebSocketEventArgs
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
        public string RawUpdateJson { get; } = null!;

        /// <summary>
        /// Event type.
        /// </summary>
        public MattermostEvent Event { get; } = MattermostEvent.Unknown;

        internal WebSocketEventArgs(MattermostClient mattermostClient, WebsocketMessage response, CancellationToken cancellationToken)
        {
            Event = response.Event;
            Client = mattermostClient;
            RawUpdateJson = response.Json;
            CancellationToken = cancellationToken;
        }
    }
}
