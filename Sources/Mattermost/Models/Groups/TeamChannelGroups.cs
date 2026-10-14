using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>Group associations indexed by channel identifier.</summary>
    public class TeamChannelGroups
    {
        /// <summary>Channel identifiers mapped to associated groups.</summary>
        [JsonPropertyName("groups")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, IList<GroupWithSchemeAdmin>>? Groups { get; set; } = new Dictionary<string, IList<GroupWithSchemeAdmin>>();
    }
}
