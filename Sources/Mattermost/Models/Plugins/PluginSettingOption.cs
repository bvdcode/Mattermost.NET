using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>A selectable plugin-setting value.</summary>
    public class PluginSettingOption
    {
        /// <summary>Display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Native string value.</summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
