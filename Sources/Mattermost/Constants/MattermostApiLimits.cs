namespace Mattermost.Constants
{
    /// <summary>
    /// Mattermost API limits, used to validate the input data.
    /// </summary>
    public static class MattermostApiLimits
    {
        /// <summary>
        /// Maximum length of the post text. <br/>
        /// https://mattermost.com/blog/mattermost-5-0-intercept-and-modify-posts-advanced-permissions-longer-posts-and-more/#:~:text=Increased%20character%20limits%20on%20posts,better%20Markdown%20formatting%2C%20including%20tables.
        /// </summary>
        public const int MaxPostMessageLength = 16383;
    }
}
