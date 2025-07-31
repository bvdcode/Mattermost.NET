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
        Direct
    }

    internal static class ChannelTypeExtensions
    {
        internal static string? ToChannelChar(this ChannelType type)
        {
            return type switch
            {
                ChannelType.Public => "O",
                ChannelType.Private => "P",
                ChannelType.Direct => "D",
                _ => null,
            };
        }
    }
}