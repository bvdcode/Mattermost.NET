using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>
    /// Defines the visibility of a slash command response.
    /// </summary>
    public enum SlashCommandResponseType
    {
        /// <summary>
        /// Displays the response only to the user who invoked the command.
        /// </summary>
        [JsonStringEnumMemberName("ephemeral")]
        Ephemeral,

        /// <summary>
        /// Posts the response to the channel where the command was invoked.
        /// </summary>
        [JsonStringEnumMemberName("in_channel")]
        InChannel,
    }
}
