namespace Mattermost.Events
{
    /// <summary>
    /// Log text message.
    /// </summary>
    public class LogEventArgs
    {
        /// <summary>
        /// Text message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Create event args with specified message.
        /// </summary>
        /// <param name="message"> Text message. </param>
        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}