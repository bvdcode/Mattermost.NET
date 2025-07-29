using System;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Teams
{
    /// <summary>
    /// Represents a team in Mattermost.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Unique identifier for the team.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds a team was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a team was last updated.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a team was deleted.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>
        /// Non-unique UI name for the team.
        /// </summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Unique handler for a team, will be present in the team URL.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the team's icon.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The email address associated with the team, used for notifications and other purposes.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The type of the team, which can be "O" for open or "I" for invite-only.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Allowed domains for the team, used for email-based user creation and login.
        /// </summary>
        [JsonPropertyName("allowed_domains")]
        public string AllowedDomains { get; set; } = string.Empty;

        /// <summary>
        /// Invitation ID for the team, used to manage team invitations.
        /// </summary>
        [JsonPropertyName("invite_id")]
        public string InviteId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the team is allowed to have an open invite link.
        /// </summary>
        [JsonPropertyName("allow_open_invite")]
        public bool AllowOpenInvite { get; set; }

        /// <summary>
        /// The data retention policy to which this team has been assigned.
        /// If no such policy exists, or the caller does not have the sysconsole_read_compliance_data_retention permission, this field will be null.
        /// </summary>
        [JsonPropertyName("policy_id")]
        public string PolicyId { get; set; } = string.Empty;
    }
}