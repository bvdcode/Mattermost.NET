using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>Structured autocomplete metadata for a plugin command or subcommand.</summary>
    public class CommandAutocompleteData
    {
        /// <summary>Command trigger.</summary>
        [JsonPropertyName("Trigger")]
        public string Trigger { get; set; } = null!;

        /// <summary>Command argument hint.</summary>
        [JsonPropertyName("Hint")]
        public string Hint { get; set; } = null!;

        /// <summary>Help description.</summary>
        [JsonPropertyName("HelpText")]
        public string HelpText { get; set; } = null!;

        /// <summary>System role that can see this command's autocomplete metadata.</summary>
        [JsonPropertyName("RoleID")]
        public string RoleId { get; set; } = null!;

        /// <summary>Named or positional arguments. Order matters for positional arguments.</summary>
        [JsonPropertyName("Arguments")]
        public IList<CommandAutocompleteArgument>? Arguments { get; set; }

        /// <summary>Subcommands, when the command uses a subcommand hierarchy.</summary>
        [JsonPropertyName("SubCommands")]
        public IList<CommandAutocompleteData>? SubCommands { get; set; }
    }
}
