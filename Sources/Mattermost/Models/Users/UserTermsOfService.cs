using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>The current user's stored acceptance of custom terms of service.</summary>
    public class UserTermsOfService
    {
        /// <summary>Account that accepted the terms.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Accepted version identifier.</summary>
        [JsonPropertyName("terms_of_service_id")]
        public string TermsOfServiceId { get; set; } = string.Empty;

        /// <summary>Acceptance time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }
    }
}
