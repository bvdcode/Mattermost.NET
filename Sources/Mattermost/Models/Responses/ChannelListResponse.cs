using Mattermost.Models.Channels;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>A page of administrative channel results with the matching total count.</summary>
    public class ChannelListResponse
    {
        /// <summary>Channel page, including native team display metadata.</summary>
        [JsonPropertyName("channels")]
        public IList<ChannelWithTeamData> Channels { get; set; } = new List<ChannelWithTeamData>();

        /// <summary>Number of all matching channels, not just this page.</summary>
        [JsonPropertyName("total_count")]
        public long TotalCount { get; set; }
    }
}
