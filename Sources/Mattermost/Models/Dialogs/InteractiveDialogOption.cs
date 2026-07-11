using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Option used by select, radio, and lookup responses.
    /// </summary>
    public class InteractiveDialogOption
    {
        /// <summary>
        /// Initializes a new option.
        /// </summary>
        public InteractiveDialogOption()
        {
        }

        /// <summary>
        /// Initializes a new option with text and value.
        /// </summary>
        /// <param name="text">Option text.</param>
        /// <param name="value">Option value.</param>
        public InteractiveDialogOption(string text, string value)
        {
            Text = text;
            Value = value;
        }

        /// <summary>
        /// Display text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Submitted value.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
