using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>Personal access token metadata and, on creation, its secret value.</summary>
    public class UserAccessToken
    {
        /// <summary>Token identifier, distinct from the secret token value.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Secret authentication value returned on creation; null on metadata reads.</summary>
        [JsonPropertyName("token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Token { get; set; }

        /// <summary>Owning user or bot identifier.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Description of the token's intended use.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Whether the token is enabled. An active token may still be expired.</summary>
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>Expiry as Unix milliseconds, or zero for a token without expiry.</summary>
        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }
}
