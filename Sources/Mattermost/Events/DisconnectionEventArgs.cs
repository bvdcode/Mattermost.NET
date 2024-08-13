using System;
using System.Net.WebSockets;

namespace Mattermost.Events
{
    /// <summary>
    /// Represents the event data for a WebSocket disconnection.
    /// </summary>
    public class DisconnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the status code indicating why the WebSocket was closed, if available.
        /// </summary>
        public WebSocketCloseStatus? CloseStatus { get; set; }

        /// <summary>
        /// Gets or sets a description of the reason for the WebSocket closure.
        /// </summary>
        public string? CloseStatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the disconnection occurred.
        /// </summary>
        public DateTime DisconnectedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectionEventArgs"/> class.
        /// </summary>
        /// <param name="closeStatus">The status code indicating why the WebSocket was closed.</param>
        /// <param name="closeStatusDescription">A description of the reason for the WebSocket closure.</param>
        /// <param name="disconnectedAt">The timestamp when the disconnection occurred.</param>
        public DisconnectionEventArgs(WebSocketCloseStatus? closeStatus, string? closeStatusDescription, DateTime disconnectedAt)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
            DisconnectedAt = disconnectedAt;
        }
    }
}