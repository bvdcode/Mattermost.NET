using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>Autocomplete argument metadata whose data shape depends on its type.</summary>
    public class CommandAutocompleteArgument
    {
        /// <summary>Argument name. Empty for positional arguments.</summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = null!;

        /// <summary>Help description.</summary>
        [JsonPropertyName("HelpText")]
        public string HelpText { get; set; } = null!;

        /// <summary>Native argument kind: TextInput, StaticList or DynamicList.</summary>
        [JsonPropertyName("Type")]
        public string Type { get; set; } = null!;

        /// <summary>Whether the argument is required.</summary>
        [JsonPropertyName("Required")]
        public bool Required { get; set; }

        /// <summary>Type-specific JSON: Hint/Pattern, PossibleArguments, or FetchURL.</summary>
        [JsonPropertyName("Data")]
        public JsonElement? Data { get; set; }
    }
}
