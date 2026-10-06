namespace Mattermost.Enums
{
    internal static class ChannelTypeExtensions
    {
        internal static string? ToChannelChar(this ChannelType type)
        {
            return type switch
            {
                ChannelType.Public => "O",
                ChannelType.Private => "P",
                ChannelType.Direct => "D",
                ChannelType.Group => "G",
                _ => null,
            };
        }
    }
}
