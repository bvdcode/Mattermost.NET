using System.Text.Json.Serialization;

namespace Mattermost.Models.Schemes
{
    /// <summary>A permission scheme and its server-created role names.</summary>
    public class Scheme
    {
        /// <summary>Scheme identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Unique scheme name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Scheme description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Native scope name.</summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>Default team administrator role name.</summary>
        [JsonPropertyName("default_team_admin_role")]
        public string DefaultTeamAdminRole { get; set; } = string.Empty;

        /// <summary>Default team user role name.</summary>
        [JsonPropertyName("default_team_user_role")]
        public string DefaultTeamUserRole { get; set; } = string.Empty;

        /// <summary>Default team guest role name.</summary>
        [JsonPropertyName("default_team_guest_role")]
        public string DefaultTeamGuestRole { get; set; } = string.Empty;

        /// <summary>Default channel administrator role name.</summary>
        [JsonPropertyName("default_channel_admin_role")]
        public string DefaultChannelAdminRole { get; set; } = string.Empty;

        /// <summary>Default channel user role name.</summary>
        [JsonPropertyName("default_channel_user_role")]
        public string DefaultChannelUserRole { get; set; } = string.Empty;

        /// <summary>Default channel guest role name.</summary>
        [JsonPropertyName("default_channel_guest_role")]
        public string DefaultChannelGuestRole { get; set; } = string.Empty;

        /// <summary>Default playbook administrator role name.</summary>
        [JsonPropertyName("default_playbook_admin_role")]
        public string DefaultPlaybookAdminRole { get; set; } = string.Empty;

        /// <summary>Default playbook member role name.</summary>
        [JsonPropertyName("default_playbook_member_role")]
        public string DefaultPlaybookMemberRole { get; set; } = string.Empty;

        /// <summary>Default run administrator role name.</summary>
        [JsonPropertyName("default_run_admin_role")]
        public string DefaultRunAdminRole { get; set; } = string.Empty;

        /// <summary>Default run member role name.</summary>
        [JsonPropertyName("default_run_member_role")]
        public string DefaultRunMemberRole { get; set; } = string.Empty;
    }
}
