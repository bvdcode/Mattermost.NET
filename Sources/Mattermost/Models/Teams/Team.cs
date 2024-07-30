using System;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Teams
{
    internal class Team
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("allowed_domains")]
        public string AllowedDomains { get; set; } = string.Empty;

        [JsonPropertyName("invite_id")]
        public string InviteId { get; set; } = string.Empty;

        [JsonPropertyName("allow_open_invite")]
        public bool AllowOpenInvite { get; set; }

        [JsonPropertyName("policy_id")]
        public string PolicyId { get; set; } = string.Empty;
    }
}