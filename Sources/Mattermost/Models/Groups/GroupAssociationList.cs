using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>Groups associated with a team or channel.</summary>
    public class GroupAssociationList
    {
        /// <summary>Associated groups and their administrator flags.</summary>
        [JsonPropertyName("groups")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<GroupWithSchemeAdmin>? Groups { get; set; } = new List<GroupWithSchemeAdmin>();

        /// <summary>Total matching group count.</summary>
        [JsonPropertyName("total_group_count")]
        public int TotalGroupCount { get; set; }
    }
}
