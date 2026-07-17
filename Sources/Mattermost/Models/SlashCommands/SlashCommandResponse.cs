using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>
    /// Represents the primary response returned for a custom slash command.
    /// </summary>
    public class SlashCommandResponse : SlashCommandResponseItem
    {
        /// <summary>
        /// The URL where Mattermost should redirect the user after processing the response.
        /// </summary>
        [JsonPropertyName("goto_location")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GotoLocation { get; set; }

        /// <summary>
        /// Additional immediate messages returned with the primary response.
        /// </summary>
        [JsonPropertyName("extra_responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<SlashCommandResponseItem>? ExtraResponses { get; set; }
    }
}
