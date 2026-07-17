using System.Collections.Generic;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>
    /// Represents the form parameters sent by Mattermost when a custom slash command is invoked.
    /// </summary>
    public class SlashCommandRequest
    {
        /// <summary>
        /// The identifier of the channel where the command was invoked.
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the channel where the command was invoked.
        /// </summary>
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// The slash command trigger, including the leading slash.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// The URL that can receive delayed or additional responses.
        /// </summary>
        public string ResponseUrl { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the root post when the command was invoked in a thread.
        /// </summary>
        public string RootId { get; set; } = string.Empty;

        /// <summary>
        /// The domain of the team where the command was invoked.
        /// </summary>
        public string TeamDomain { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the team where the command was invoked.
        /// </summary>
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// The arguments supplied after the slash command trigger.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The verification token configured for the slash command.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The trigger identifier that can be used to open an interactive dialog.
        /// </summary>
        public string TriggerId { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the user who invoked the command.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The username of the user who invoked the command.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Usernames mentioned in the command text, mapped to their user identifiers.
        /// </summary>
        public IReadOnlyDictionary<string, string> UserMentions { get; set; } =
            new Dictionary<string, string>();

        /// <summary>
        /// Channel names mentioned in the command text, mapped to their channel identifiers.
        /// </summary>
        public IReadOnlyDictionary<string, string> ChannelMentions { get; set; } =
            new Dictionary<string, string>();
    }
}
