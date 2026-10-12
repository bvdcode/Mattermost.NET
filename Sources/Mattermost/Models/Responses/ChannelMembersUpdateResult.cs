using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>One batch of results from a declarative channel membership update.</summary>
    public class ChannelMembersUpdateResult
    {
        /// <summary>Identifiers of users successfully added in this batch.</summary>
        [JsonPropertyName("added")]
        public IList<string> Added { get; set; } = new List<string>();

        /// <summary>Identifiers of users successfully removed in this batch.</summary>
        [JsonPropertyName("removed")]
        public IList<string> Removed { get; set; } = new List<string>();

        /// <summary>Identifiers of users granted the channel administrator role.</summary>
        [JsonPropertyName("promoted")]
        public IList<string> Promoted { get; set; } = new List<string>();

        /// <summary>Identifiers of users whose channel administrator role was removed.</summary>
        [JsonPropertyName("demoted")]
        public IList<string> Demoted { get; set; } = new List<string>();

        /// <summary>Per-user failures. Other operations in the batch may already have succeeded.</summary>
        [JsonPropertyName("errors")]
        public IList<ChannelMemberUpdateError> Errors { get; set; } = new List<ChannelMemberUpdateError>();
    }
}
