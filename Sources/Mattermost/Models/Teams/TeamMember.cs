using System.Text.Json.Serialization;

namespace Mattermost.Models.Teams
{
    /// <summary>
    /// A user's membership and roles in a team.
    /// </summary>
    public class TeamMember
    {
        /// <summary>
        /// Team identifier.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Space-separated effective team roles, including scheme roles. May be hidden by the server.
        /// </summary>
        [JsonPropertyName("roles")]
        public string Roles { get; set; } = null!;

        /// <summary>
        /// Membership deletion time as a Unix timestamp in milliseconds, zero for an active membership, or -1 when hidden.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>
        /// Whether the guest role is granted by the team's permissions scheme.
        /// </summary>
        [JsonPropertyName("scheme_guest")]
        public bool SchemeGuest { get; set; }

        /// <summary>
        /// Whether the user role is granted by the team's permissions scheme.
        /// </summary>
        [JsonPropertyName("scheme_user")]
        public bool SchemeUser { get; set; }

        /// <summary>
        /// Whether the administrator role is granted by the team's permissions scheme.
        /// </summary>
        [JsonPropertyName("scheme_admin")]
        public bool SchemeAdmin { get; set; }

        /// <summary>
        /// Space-separated roles assigned explicitly, excluding scheme roles. May be hidden by the server.
        /// </summary>
        [JsonPropertyName("explicit_roles")]
        public string ExplicitRoles { get; set; } = null!;
    }
}
