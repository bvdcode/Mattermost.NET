using Mattermost.Models.Teams;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>User session metadata. The server does not return usable session tokens.</summary>
    public class UserSession
    {
        /// <summary>Session identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Session creation time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Session expiry as a Unix timestamp in milliseconds. A nonpositive value means no expiry.</summary>
        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }

        /// <summary>Last activity time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("last_activity_at")]
        public long LastActivityAt { get; set; }

        /// <summary>User identifier.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>Associated device identifier.</summary>
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = null!;

        /// <summary>Associated voice notification device identifier.</summary>
        [JsonPropertyName("voip_device_id")]
        public string VoipDeviceId { get; set; } = null!;

        /// <summary>Space-separated session roles.</summary>
        [JsonPropertyName("roles")]
        public string Roles { get; set; } = null!;

        /// <summary>Whether the session was created through OAuth.</summary>
        [JsonPropertyName("is_oauth")]
        public bool IsOAuth { get; set; }

        /// <summary>Whether expiry notification has been recorded.</summary>
        [JsonPropertyName("expired_notify")]
        public bool ExpiredNotify { get; set; }

        /// <summary>Session properties, such as platform, browser and access-token identifier.</summary>
        [JsonPropertyName("props")]
        public IDictionary<string, string>? Props { get; set; }

        /// <summary>Team membership metadata when returned by the server.</summary>
        [JsonPropertyName("team_members")]
        public IList<TeamMember>? TeamMembers { get; set; }

        /// <summary>Whether this is a local-mode session.</summary>
        [JsonPropertyName("local")]
        public bool Local { get; set; }
    }
}
