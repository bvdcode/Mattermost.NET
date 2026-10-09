using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Roles
{
    /// <summary>
    /// A role and the permissions it grants.
    /// </summary>
    public class Role
    {
        /// <summary>Role identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Unique role name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>Display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;

        /// <summary>Role description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Permission identifiers granted by the role.</summary>
        [JsonPropertyName("permissions")]
        public IList<string> Permissions { get; set; } = new List<string>();

        /// <summary>Whether a permission scheme manages this role.</summary>
        [JsonPropertyName("scheme_managed")]
        public bool IsSchemeManaged { get; set; }

        /// <summary>Whether this is a built-in role.</summary>
        [JsonPropertyName("built_in")]
        public bool IsBuiltIn { get; set; }

        /// <summary>Permission scheme identifier, when associated with a scheme.</summary>
        [JsonPropertyName("scheme_id")]
        public string? SchemeId { get; set; }
    }
}
