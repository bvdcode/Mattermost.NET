using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Represents a selectable option for a select post action.
    /// </summary>
    public class PostActionOption
    {
        /// <summary>
        /// Initializes a new empty post action option.
        /// </summary>
        public PostActionOption()
        {
        }

        /// <summary>
        /// Initializes a new post action option.
        /// </summary>
        public PostActionOption(string text, string value)
        {
            Text = text;
            Value = value;
        }

        /// <summary>
        /// The option text shown to the user.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The option value sent to the action integration.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
