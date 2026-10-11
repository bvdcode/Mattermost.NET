using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>A registered slash command or a built-in/plugin command definition.</summary>
    public class SlashCommand
    {
        /// <summary>Command identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Secret used to validate requests to the custom command's endpoint. May be hidden in listings.</summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;

        /// <summary>Creation time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Update time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time as a Unix timestamp in milliseconds, zero when active.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Creator's user identifier. Empty for plugin commands or when hidden by the server.</summary>
        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Team identifier.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

        /// <summary>Command trigger without a leading slash.</summary>
        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = null!;

        /// <summary>Request method: P for POST or G for GET. May be hidden in autocomplete listings.</summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = "P";

        /// <summary>Username override for posts produced by the command.</summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Icon URL override for posts produced by the command.</summary>
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        /// <summary>Whether the command appears in autocomplete.</summary>
        [JsonPropertyName("auto_complete")]
        public bool AutoComplete { get; set; }

        /// <summary>Description displayed during autocomplete.</summary>
        [JsonPropertyName("auto_complete_desc")]
        public string AutoCompleteDescription { get; set; } = string.Empty;

        /// <summary>Argument hint displayed during autocomplete.</summary>
        [JsonPropertyName("auto_complete_hint")]
        public string AutoCompleteHint { get; set; } = string.Empty;

        /// <summary>Display name in command management.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Description in command management.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Absolute HTTP or HTTPS request URL. May be hidden in autocomplete listings.</summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        /// <summary>Identifier of the plugin providing the command, if any.</summary>
        [JsonPropertyName("plugin_id")]
        public string PluginId { get; set; } = string.Empty;

        /// <summary>Structured autocomplete information provided by a plugin.</summary>
        [JsonPropertyName("autocomplete_data")]
        public CommandAutocompleteData? AutocompleteData { get; set; }

        /// <summary>Base64-encoded SVG autocomplete icon provided by a plugin.</summary>
        [JsonPropertyName("autocomplete_icon_data")]
        public string? AutocompleteIconData { get; set; }
    }
}
