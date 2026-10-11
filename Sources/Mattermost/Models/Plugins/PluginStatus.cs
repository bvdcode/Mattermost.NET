using Mattermost.Enums;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>Cluster-aware runtime state of an installed plugin.</summary>
    public class PluginStatus
    {
        /// <summary>Plugin identifier.</summary>
        [JsonPropertyName("plugin_id")]
        public string PluginId { get; set; } = string.Empty;

        /// <summary>Cluster node identifier.</summary>
        [JsonPropertyName("cluster_id")]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>Plugin directory on the server.</summary>
        [JsonPropertyName("plugin_path")]
        public string PluginPath { get; set; } = string.Empty;

        /// <summary>Native numeric state; unrecognized numeric values are preserved.</summary>
        [JsonPropertyName("state")]
        public PluginState State { get; set; }

        /// <summary>Runtime error, or empty when absent.</summary>
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        /// <summary>Display name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Plugin version.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}
