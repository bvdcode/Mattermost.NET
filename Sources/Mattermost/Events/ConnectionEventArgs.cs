using System;

namespace Mattermost.Events
{
    /// <summary>
    /// Represents the event data for a successful WebSocket connection.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the URI to which the WebSocket is connected.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the connection was established.
        /// </summary>
        public DateTime ConnectedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="uri">The URI to which the WebSocket is connected.</param>
        /// <param name="connectedAt">The timestamp when the connection was established.</param>
        public ConnectionEventArgs(Uri uri, DateTime connectedAt)
        {
            Uri = uri;
            ConnectedAt = connectedAt;
        }
    }
}