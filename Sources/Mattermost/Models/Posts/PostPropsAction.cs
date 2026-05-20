using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Represents an action that can be performed in a post, such as a button click or menu selection.
    /// </summary>
    public class PostPropsAction
    {
        /// <summary>
        /// The unique identifier for this action.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The interactive element type.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PostActionType? Type { get; set; }

        /// <summary>
        /// The text on the button, or in the select placeholder.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The data source used to populate a select action.
        /// </summary>
        [JsonPropertyName("data_source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PostActionDataSource? DataSource { get; set; }

        /// <summary>
        /// The static options listed in a select action.
        /// </summary>
        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<PostActionOption>? Options { get; set; }

        /// <summary>
        /// The option value that appears as the default selection in a select action.
        /// </summary>
        [JsonPropertyName("default_option")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultOption { get; set; }

        /// <summary>
        /// Integration details associated with this action.
        /// </summary>
        [JsonPropertyName("integration")]
        public Integration Integration { get; set; } = new Integration();

        /// <summary>
        /// The visual style of the action.
        /// </summary>
        [JsonPropertyName("style")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActionStyle? Style { get; set; }
    }
}
