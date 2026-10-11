using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>
    /// Represents the primary slash command response, including command execution metadata.
    /// </summary>
    public class SlashCommandResponse : SlashCommandResponseItem
    {
        /// <summary>
        /// Client trigger identifier returned by command execution, when provided by the server.
        /// </summary>
        [JsonPropertyName("trigger_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TriggerId { get; set; }

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
