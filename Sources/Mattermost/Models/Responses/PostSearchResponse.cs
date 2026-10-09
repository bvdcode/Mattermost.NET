using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Posts matching a search, with optional matched terms per post.</summary>
    public class PostSearchResponse : ChannelPostsResponse
    {
        /// <summary>Matched terms by post identifier, or null when the search backend does not provide them.</summary>
        [JsonPropertyName("matches")]
        public IReadOnlyDictionary<string, IReadOnlyList<string>>? Matches { get; set; }
    }
}
