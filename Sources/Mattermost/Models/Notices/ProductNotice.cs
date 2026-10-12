using System.Text.Json.Serialization;

namespace Mattermost.Models.Notices
{
    /// <summary>A product notice selected by the server for the current user.</summary>
    public class ProductNotice
    {
        /// <summary>Notice identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Notice title; may contain Markdown and the {{Mattermost}} branding placeholder.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Notice content; may contain Markdown and the {{Mattermost}} branding placeholder.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <summary>Optional button action. An absent action closes the notice.</summary>
        [JsonPropertyName("action")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ProductNoticeAction? Action { get; set; }

        /// <summary>Optional action parameter, such as a URL.</summary>
        [JsonPropertyName("actionParam")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ActionParam { get; set; }

        /// <summary>Optional button label; the server may leave the default label to the client.</summary>
        [JsonPropertyName("actionText")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ActionText { get; set; }

        /// <summary>Optional image supplied with the notice.</summary>
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Image { get; set; }

        /// <summary>Whether the notice targets system administrators only.</summary>
        [JsonPropertyName("sysAdminOnly")]
        public bool SysAdminOnly { get; set; }

        /// <summary>Whether the notice targets team administrators only.</summary>
        [JsonPropertyName("teamAdminOnly")]
        public bool TeamAdminOnly { get; set; }
    }
}
