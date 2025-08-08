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
        /// The name of the action, which is displayed to the user.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Integration details associated with this action.
        /// </summary>
        [JsonPropertyName("integration")]
        public Integration Integration { get; set; } = new Integration();
    }
}
