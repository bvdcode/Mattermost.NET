using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>A version of the server's custom terms of service.</summary>
    public class TermsOfService
    {
        /// <summary>Published version identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Publication time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Account that published this version.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Custom terms text.</summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
