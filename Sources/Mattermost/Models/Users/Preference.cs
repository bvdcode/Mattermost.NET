using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>A stored user preference, identified by user, category, and name.</summary>
    public class Preference
    {
        /// <summary>Actual user identifier, not the "me" alias.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>Preference category.</summary>
        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        /// <summary>Preference name; some categories use an empty name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Preference value; whitespace and empty strings are significant.</summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
