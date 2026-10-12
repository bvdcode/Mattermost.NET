namespace Mattermost.Constants
{
    internal static class Routes
    {
        private const string version = "/api/v4";
        internal const string Plugins = "/plugins";
        internal const string CallsPlugin = Plugins + "/com.mattermost.calls";
        internal const string Dialogs = version + "/actions/dialogs";
        internal const string Users = version + "/users";
        internal const string Bots = version + "/bots";
        internal const string Jobs = version + "/jobs";
        internal const string CorePlugins = version + "/plugins";
        internal const string UserAccessTokens = Users + "/tokens";
        internal const string Posts = version + "/posts";
        internal const string Drafts = version + "/drafts";
        internal const string Commands = version + "/commands";
        internal const string OAuthApps = version + "/oauth/apps";
        internal const string OAuthServerMetadata = "/.well-known/oauth-authorization-server";
        internal const string IncomingWebhooks = version + "/hooks/incoming";
        internal const string OutgoingWebhooks = version + "/hooks/outgoing";
        internal const string Reactions = version + "/reactions";
        internal const string Emojis = version + "/emoji";
        internal const string Roles = version + "/roles";
        internal const string Teams = version + "/teams";
        internal const string Files = version + "/files";
        internal const string Uploads = version + "/uploads";
        internal const string System = version + "/system";
        internal const string ServerLogs = version + "/logs";
        internal const string ServerAudits = version + "/audits";
        internal const string ServerAnalytics = version + "/analytics/old";
        internal const string ServerBusy = version + "/server_busy";
        internal const string Config = version + "/config";
        internal const string SiteUrlTest = version + "/site_url/test";
        internal const string FileStoreTest = version + "/file/test";
        internal const string LatestVersion = version + "/latest_version";
        internal const string ClientConfig = version + "/config/client";
        internal const string License = version + "/license";
        internal const string Channels = version + "/channels";
        internal const string WebSocket = version + "/websocket";
        internal const string GroupChannels = Channels + "/group";
        internal const string DefaultBaseUrl = "https://community.mattermost.com";
    }
}
