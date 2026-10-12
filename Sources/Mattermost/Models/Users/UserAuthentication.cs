using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>User authentication metadata returned after an administrative authentication update.</summary>
    public class UserAuthentication
    {
        /// <summary>External authentication identifier, or null for email/password authentication.</summary>
        [JsonPropertyName("auth_data")]
        public string? AuthData { get; set; }

        /// <summary>Native provider name. Empty represents email/password authentication in the update response.</summary>
        [JsonPropertyName("auth_service")]
        public string AuthService { get; set; } = string.Empty;
    }
}
