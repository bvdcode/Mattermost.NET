using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>User suggestions matching an autocomplete query and its team/channel filters.</summary>
    public class UserAutocomplete
    {
        /// <summary>Matching users, restricted to the channel or team when specified.</summary>
        [JsonPropertyName("users")]
        public IList<User>? Users { get; set; }

        /// <summary>Matching users outside the requested channel. Omitted when no channel is specified.</summary>
        [JsonPropertyName("out_of_channel")]
        public IList<User>? OutOfChannel { get; set; }

        /// <summary>Agent user suggestions, when supported and returned by the server.</summary>
        [JsonPropertyName("agents")]
        public IList<User>? Agents { get; set; }
    }
}
