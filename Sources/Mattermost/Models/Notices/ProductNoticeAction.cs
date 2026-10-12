using System.Text.Json.Serialization;

namespace Mattermost.Models.Notices
{
    /// <summary>Actions supported by a product-notice button.</summary>
    public enum ProductNoticeAction
    {
        /// <summary>Open the URL provided by the notice.</summary>
        [JsonStringEnumMemberName("url")]
        Url
    }
}
