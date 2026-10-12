using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>An applied Mattermost database schema migration.</summary>
    public class AppliedSchemaMigration
    {
        /// <summary>Native migration version.</summary>
        [JsonPropertyName("version")]
        public long Version { get; set; }

        /// <summary>Native migration name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
