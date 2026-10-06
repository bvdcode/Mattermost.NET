namespace Mattermost.Enums
{
    /// <summary>
    /// Channel type.
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Public channel - anyone can join.
        /// </summary>
        Public,

        /// <summary>
        /// Private channel - join only by invitation.
        /// </summary>
        Private,

        /// <summary>
        /// Direct channel - private channel with only two participants.
        /// </summary>
        Direct,

        /// <summary>
        /// Group message channel with multiple participants.
        /// </summary>
        Group
    }
}
