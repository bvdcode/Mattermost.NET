using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Mattermost.Models.Plugins
{
    /// <summary>Metadata and runtime components declared by a plugin.</summary>
    public class PluginManifest
    {
        /// <summary>Globally unique plugin identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Plugin version.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Optional homepage URL.</summary>
        [JsonPropertyName("homepage_url")]
        public string? HomepageUrl { get; set; }

        /// <summary>Optional support URL.</summary>
        [JsonPropertyName("support_url")]
        public string? SupportUrl { get; set; }

        /// <summary>Optional release notes URL.</summary>
        [JsonPropertyName("release_notes_url")]
        public string? ReleaseNotesUrl { get; set; }

        /// <summary>Relative icon path within the bundle.</summary>
        [JsonPropertyName("icon_path")]
        public string? IconPath { get; set; }

        /// <summary>Minimum supported server version.</summary>
        [JsonPropertyName("min_server_version")]
        public string? MinServerVersion { get; set; }

        /// <summary>Required license add-on, when declared.</summary>
        [JsonPropertyName("required_add_on")]
        public string? RequiredAddOn { get; set; }

        /// <summary>Server-side executable paths.</summary>
        [JsonPropertyName("server")]
        public PluginServerManifest? Server { get; set; }

        /// <summary>Web application bundle metadata.</summary>
        [JsonPropertyName("webapp")]
        public PluginWebappManifest? Webapp { get; set; }

        /// <summary>Setting definitions; omitted by the webapp listing.</summary>
        [JsonPropertyName("settings_schema")]
        public PluginSettingsSchema? SettingsSchema { get; set; }

        /// <summary>Plugin-defined JSON values shared with other plugins.</summary>
        [JsonPropertyName("props")]
        public IDictionary<string, JsonElement>? Props { get; set; }
    }
}
