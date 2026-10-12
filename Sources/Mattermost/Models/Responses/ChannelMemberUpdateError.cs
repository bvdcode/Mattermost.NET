using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>A user's failed operation within a channel membership update.</summary>
    public class ChannelMemberUpdateError
    {
        /// <summary>User whose membership or role could not be changed.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Native server error identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Native server error text.</summary>
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
    }
}
