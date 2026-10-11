using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>Sidebar categories and their display order for a user on a team.</summary>
    public class OrderedSidebarCategories
    {
        /// <summary>Categories, including their ordered channel identifiers.</summary>
        [JsonPropertyName("categories")]
        public IList<SidebarCategory> Categories { get; set; } = new List<SidebarCategory>();

        /// <summary>Category identifiers in display order.</summary>
        [JsonPropertyName("order")]
        public IList<string> Order { get; set; } = new List<string>();
    }
}
