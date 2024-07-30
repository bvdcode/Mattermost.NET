namespace Mattermost.Constants
{
    internal static class Routes
    {
        internal const string DefaultBaseUrl = "https://community.mattermost.com/";
        internal const string Users = "/api/v4/users";
        internal const string WebSocket = "api/v4/websocket";
        internal const string Posts = "/api/v4/posts";
        internal const string Teams = "/api/v4/teams";
        internal const string Files = "/api/v4/files";
        internal const string Plugins = "/plugins";
        internal const string Channels = "/api/v4/channels";
        internal const string GroupChannels = Channels + "/group";
    }
}