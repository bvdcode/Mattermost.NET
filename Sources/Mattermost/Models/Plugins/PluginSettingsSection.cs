using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>A group of plugin settings displayed in the System Console.</summary>
    public class PluginSettingsSection
    {
        /// <summary>Section identifier.</summary>
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        /// <summary>Section title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Section subtitle.</summary>
        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>Setting definitions in the section.</summary>
        [JsonPropertyName("settings")]
        public IList<PluginSetting>? Settings { get; set; }

        /// <summary>Header text supporting Markdown.</summary>
        [JsonPropertyName("header")]
        public string Header { get; set; } = string.Empty;

        /// <summary>Footer text supporting Markdown.</summary>
        [JsonPropertyName("footer")]
        public string Footer { get; set; } = string.Empty;

        /// <summary>Whether the section uses a custom webapp component.</summary>
        [JsonPropertyName("custom")]
        public bool IsCustom { get; set; }

        /// <summary>Whether ordinary settings appear when a custom section's plugin is disabled.</summary>
        [JsonPropertyName("fallback")]
        public bool Fallback { get; set; }
    }
}
