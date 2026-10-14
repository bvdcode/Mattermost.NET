using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>A group and its administrator status for an associated resource.</summary>
    public class GroupWithSchemeAdmin : Group
    {
        /// <summary>Whether group members receive the resource administrator role.</summary>
        [JsonPropertyName("scheme_admin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SchemeAdmin { get; set; }
    }
}
