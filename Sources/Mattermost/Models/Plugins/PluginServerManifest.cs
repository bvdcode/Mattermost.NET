using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>Server-side executable paths declared by a plugin.</summary>
    public class PluginServerManifest
    {
        /// <summary>Single executable path relative to the bundle.</summary>
        [JsonPropertyName("executable")]
        public string Executable { get; set; } = string.Empty;

        /// <summary>Platform names mapped to executable paths.</summary>
        [JsonPropertyName("executables")]
        public IDictionary<string, string>? Executables { get; set; }
    }
}
