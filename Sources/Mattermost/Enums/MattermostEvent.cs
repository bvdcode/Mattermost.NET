namespace Mattermost.Enums
{
    /// <summary>
    /// Event types which can be received from Mattermost server through WebSocket.
    /// </summary>
    public enum MattermostEvent
    {
        /// <summary>
        /// Unknown event type (not defined in this enum).
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Called when a new post is created.
        /// </summary>
        Posted = 1,

        /// <summary>
        /// Called when user status changed.
        /// </summary>
        StatusChange = 2,

        /// <summary>
        /// Called when user is typing text.
        /// </summary>
        Typing = 3,

        /// <summary>
        /// Called when user viewed multiple channels.
        /// </summary>
        MultipleChannelsViewed = 4,

        /// <summary>
        /// Called when user preferences changed.
        /// </summary>
        PreferencesChanged = 5,

        /// <summary>
        /// Called when sidebar category updated.
        /// </summary>
        SidebarCategoryUpdated = 6,

        /// <summary>
        /// Called when user added to channel.
        /// </summary>
        UserAdded = 7,

        /// <summary>
        /// Called when ephemeral message received, e.g. when post is edited.
        /// </summary>
        EphemeralMessage = 8,

        /// <summary>
        /// Called when user updated.
        /// </summary>
        UserUpdated = 9,
    }
}