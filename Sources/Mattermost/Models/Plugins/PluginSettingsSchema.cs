using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>System Console setting definitions declared by a plugin.</summary>
    public class PluginSettingsSchema
    {
        /// <summary>Header text supporting Markdown.</summary>
        [JsonPropertyName("header")]
        public string Header { get; set; } = string.Empty;

        /// <summary>Footer text supporting Markdown.</summary>
        [JsonPropertyName("footer")]
        public string Footer { get; set; } = string.Empty;

        /// <summary>Top-level setting definitions.</summary>
        [JsonPropertyName("settings")]
        public IList<PluginSetting>? Settings { get; set; }

        /// <summary>Grouped setting definitions.</summary>
        [JsonPropertyName("sections")]
        public IList<PluginSettingsSection>? Sections { get; set; }
    }
}
