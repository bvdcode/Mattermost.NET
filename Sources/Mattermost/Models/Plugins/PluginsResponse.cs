using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>Installed plugin manifests grouped by activation state.</summary>
    public class PluginsResponse
    {
        /// <summary>Active plugin manifests.</summary>
        [JsonPropertyName("active")]
        public IList<PluginManifest> Active { get; set; } = new List<PluginManifest>();

        /// <summary>Inactive plugin manifests.</summary>
        [JsonPropertyName("inactive")]
        public IList<PluginManifest> Inactive { get; set; } = new List<PluginManifest>();
    }
}
