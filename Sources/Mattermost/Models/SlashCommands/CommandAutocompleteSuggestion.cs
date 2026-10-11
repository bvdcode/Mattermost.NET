using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>A completed command or argument suggestion.</summary>
    public class CommandAutocompleteSuggestion
    {
        /// <summary>Completed command input.</summary>
        [JsonPropertyName("Complete")]
        public string Complete { get; set; } = null!;

        /// <summary>Suggested command or argument.</summary>
        [JsonPropertyName("Suggestion")]
        public string Suggestion { get; set; } = null!;

        /// <summary>Argument hint.</summary>
        [JsonPropertyName("Hint")]
        public string Hint { get; set; } = null!;

        /// <summary>Description of the suggestion.</summary>
        [JsonPropertyName("Description")]
        public string Description { get; set; } = null!;

        /// <summary>Base64-encoded SVG icon data.</summary>
        [JsonPropertyName("IconData")]
        public string IconData { get; set; } = null!;
    }
}
