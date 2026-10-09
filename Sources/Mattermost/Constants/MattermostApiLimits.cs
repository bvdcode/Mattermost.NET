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

        /// <summary>
        /// Maximum number of post IDs accepted by a single bulk lookup.
        /// </summary>
        public const int MaxPostIdsPerRequest = 1000;

        /// <summary>
        /// Maximum number of channel memberships returned on a single page.
        /// </summary>
        public const int MaxChannelMembersPerPage = 200;

        /// <summary>
        /// Maximum number of team memberships returned on a single page.
        /// </summary>
        public const int MaxTeamMembersPerPage = 200;

        /// <summary>
        /// Maximum number of custom emojis returned on a single page.
        /// </summary>
        public const int MaxEmojisPerPage = 200;

        /// <summary>
        /// Maximum number of distinct emoji names accepted by a single bulk lookup.
        /// </summary>
        public const int MaxEmojiNamesPerRequest = 200;

        /// <summary>
        /// Maximum number of distinct role names accepted by a bulk lookup.
        /// </summary>
        public const int MaxRoleNamesPerRequest = 100;

        /// <summary>Maximum number of preferences saved or deleted in one request.</summary>
        public const int MaxPreferencesPerRequest = 100;

        /// <summary>Maximum number of channels returned on a page.</summary>
        public const int MaxChannelsPerPage = 200;

        /// <summary>Maximum number of posts on either side of the unread boundary.</summary>
        public const int MaxPostsAroundUnread = 200;
    }
}
