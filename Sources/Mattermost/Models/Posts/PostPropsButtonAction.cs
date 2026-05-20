namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Convenience action model for message buttons.
    /// </summary>
    public class PostPropsButtonAction : PostPropsAction
    {
        /// <summary>
        /// Initializes a new button action.
        /// </summary>
        public PostPropsButtonAction()
        {
            Type = PostActionType.Button;
        }
    }
}
