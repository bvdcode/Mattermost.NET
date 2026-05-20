namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Convenience action model for select message menus.
    /// </summary>
    public class PostPropsSelectAction : PostPropsAction
    {
        /// <summary>
        /// Initializes a new select action.
        /// </summary>
        public PostPropsSelectAction()
        {
            Type = PostActionType.Select;
        }
    }
}
