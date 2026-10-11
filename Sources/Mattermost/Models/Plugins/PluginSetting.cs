using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Mattermost.Models.Plugins
{
    /// <summary>A plugin configuration setting definition, not its current configured value.</summary>
    public class PluginSetting
    {
        /// <summary>Configuration key.</summary>
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        /// <summary>Display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Native setting type, such as bool, text, number or custom.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Help text supporting Markdown.</summary>
        [JsonPropertyName("help_text")]
        public string HelpText { get; set; } = string.Empty;

        /// <summary>Optional regeneration-button help text.</summary>
        [JsonPropertyName("regenerate_help_text")]
        public string? RegenerateHelpText { get; set; }

        /// <summary>Input placeholder.</summary>
        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        /// <summary>Plugin-defined default JSON value.</summary>
        [JsonPropertyName("default")]
        public JsonElement Default { get; set; }

        /// <summary>Selectable options for dropdown and radio settings.</summary>
        [JsonPropertyName("options")]
        public IList<PluginSettingOption>? Options { get; set; }

        /// <summary>Native hosting restriction.</summary>
        [JsonPropertyName("hosting")]
        public string Hosting { get; set; } = string.Empty;

        /// <summary>Whether the configured value should be hidden by the server.</summary>
        [JsonPropertyName("secret")]
        public bool IsSecret { get; set; }
    }
}
