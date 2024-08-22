namespace Mattermost.Constants
{
    internal static class Routes
    {
        private const string version = "/api/v4";
        internal const string Plugins = "/plugins";
        internal const string Users = version + "/users";
        internal const string Posts = version + "/posts";
        internal const string Teams = version + "/teams";
        internal const string Files = version + "/files";
        internal const string Channels = version + "/channels";
        internal const string WebSocket = version + "/websocket";
        internal const string GroupChannels = Channels + "/group";
        internal const string DefaultBaseUrl = "https://community.mattermost.com";
    }
}