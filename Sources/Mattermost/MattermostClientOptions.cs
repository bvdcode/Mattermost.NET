using Mattermost.Events;
using System;

namespace Mattermost
{
    /// <summary>
    /// Configuration options for <see cref="MattermostClient"/> behavior.
    /// </summary>
    public class MattermostClientOptions
    {
        /// <summary>
        /// When true, messages authored by the currently authorized user are ignored by
        /// <see cref="MattermostClient.OnMessageReceived"/>.
        /// </summary>
        public bool IgnoreOwnMessages { get; set; } = true;

        /// <summary>
        /// Optional additional predicate to filter messages before
        /// <see cref="MattermostClient.OnMessageReceived"/> is invoked.
        /// Return true to dispatch the event; otherwise false.
        /// </summary>
        public Func<MessageEventArgs, bool>? IncomingMessageFilter { get; set; }

        internal bool ShouldDispatchMessage(MessageEventArgs messageEventArgs)
        {
            if (IgnoreOwnMessages && messageEventArgs.IsCurrentUser)
            {
                return false;
            }

            if (IncomingMessageFilter != null)
            {
                return IncomingMessageFilter(messageEventArgs);
            }

            return true;
        }
    }
}
