using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>Web application component of a plugin.</summary>
    public class PluginWebappManifest
    {
        /// <summary>Bundle path; the webapp listing returns its resolved server path.</summary>
        [JsonPropertyName("bundle_path")]
        public string BundlePath { get; set; } = string.Empty;
    }
}
