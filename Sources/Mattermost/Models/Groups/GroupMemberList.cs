using Mattermost.Models.Users;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>A page of group members and its total.</summary>
    public class GroupMemberList
    {
        /// <summary>User profiles visible to the caller.</summary>
        [JsonPropertyName("members")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<User>? Members { get; set; } = new List<User>();

        /// <summary>Total visible member count.</summary>
        [JsonPropertyName("total_member_count")]
        public int TotalMemberCount { get; set; }
    }
}
