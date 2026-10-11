using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>A user's sidebar category and its ordered channel identifiers.</summary>
    public class SidebarCategory
    {
        /// <summary>Category identifier. Built-in category identifiers are not ordinary channel IDs.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Actual user identifier, not the me alias.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Team identifier.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>Server-owned category position. Change category order with UpdateSidebarCategoryOrderAsync.</summary>
        [JsonPropertyName("sort_order")]
        public long SortOrder { get; set; }

        /// <summary>Channel sorting mode: empty for the default, manual, recent, or alpha.</summary>
        [JsonPropertyName("sorting")]
        public string Sorting { get; set; } = string.Empty;

        /// <summary>Category type: channels, direct_messages, favorites, custom, or managed. Cannot be changed by an update.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Category display name. Only custom category names can be updated.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Whether this category is muted.</summary>
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }

        /// <summary>Whether this category is collapsed.</summary>
        [JsonPropertyName("collapsed")]
        public bool Collapsed { get; set; }

        /// <summary>Ordered channel identifiers. The server may return null for an empty category.</summary>
        [JsonPropertyName("channel_ids")]
        public IList<string>? ChannelIds { get; set; } = new List<string>();
    }
}
