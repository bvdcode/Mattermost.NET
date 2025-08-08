using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Represents an integration in a post, which can include a URL and context information.
    /// </summary>
    public class Integration
    {
        /// <summary>
        /// The URL associated with the integration, which can be used to trigger actions or fetch data.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// A dictionary containing context information for the integration.
        /// </summary>
        [JsonPropertyName("context")]
        public IDictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }
}
