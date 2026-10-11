using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Plugins;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PluginReadTests
    {
        private const string ManifestJson = "{\"id\":\"plugin\",\"name\":\"Plugin\",\"description\":\"description\",\"homepage_url\":\"home\",\"support_url\":\"support\",\"release_notes_url\":\"notes\",\"icon_path\":\"icon.svg\",\"version\":\"1.2.3\",\"min_server_version\":\"12.0\",\"required_add_on\":\"addon\",\"server\":{\"executable\":\"server/plugin\",\"executables\":{\"linux-amd64\":\"server/linux\"}},\"webapp\":{\"bundle_path\":\"webapp/main.js\"},\"props\":{\"enabled\":true,\"count\":3,\"nested\":{\"value\":\"text\"}},\"settings_schema\":{\"header\":\"header\",\"footer\":\"footer\",\"settings\":[],\"sections\":[{\"key\":\"section\",\"title\":\"Title\",\"subtitle\":\"Subtitle\",\"header\":\"section header\",\"footer\":\"section footer\",\"custom\":true,\"fallback\":true,\"settings\":[{\"key\":\"setting\",\"display_name\":\"Setting\",\"type\":\"dropdown\",\"help_text\":\"help\",\"regenerate_help_text\":\"regenerate\",\"placeholder\":\"placeholder\",\"default\":{\"value\":[1,true]},\"hosting\":\"cloud\",\"secret\":true,\"options\":[{\"display_name\":\"Option\",\"value\":\"option\"}]}]}]}}";

        [TestCase("installed", "/chat/api/v4/plugins", "{\"active\":[],\"inactive\":[]}")]
        [TestCase("webapp", "/chat/api/v4/plugins/webapp", "[]")]
        [TestCase("statuses", "/chat/api/v4/plugins/statuses", "[]")]
        [TestCase("marketplace", "/chat/api/v4/plugins/marketplace?local_only=true&remote_only=false&filter=search%26value", "[]")]
        [TestCase("visited", "/chat/api/v4/plugins/marketplace/first_admin_visit", "{\"name\":\"FirstAdminVisitMarketplace\",\"value\":\"true\"}")]
        public async Task Reads_UseNativeRoutes(string operation, string route, string json)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await ReadAsync(client, operation);
        }

        [Test]
        public async Task InstalledManifests_IncludeTypedNestedDefinitionsAndJsonValues()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "{\"active\":[" + ManifestJson + "],\"inactive\":[{\"id\":\"inactive\",\"name\":\"Inactive\",\"version\":\"1\"}]}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            PluginsResponse response = await client.GetPluginsAsync();
            PluginManifest manifest = response.Active.Single();
            Assert.Multiple(() =>
            {
                Assert.That(manifest.Id, Is.EqualTo("plugin"));
                Assert.That(manifest.Name, Is.EqualTo("Plugin"));
                Assert.That(manifest.Description, Is.EqualTo("description"));
                Assert.That(manifest.HomepageUrl, Is.EqualTo("home"));
                Assert.That(manifest.SupportUrl, Is.EqualTo("support"));
                Assert.That(manifest.ReleaseNotesUrl, Is.EqualTo("notes"));
                Assert.That(manifest.IconPath, Is.EqualTo("icon.svg"));
                Assert.That(manifest.Version, Is.EqualTo("1.2.3"));
                Assert.That(manifest.MinServerVersion, Is.EqualTo("12.0"));
                Assert.That(manifest.RequiredAddOn, Is.EqualTo("addon"));
                Assert.That(manifest.Server!.Executable, Is.EqualTo("server/plugin"));
                Assert.That(manifest.Server.Executables!["linux-amd64"], Is.EqualTo("server/linux"));
                Assert.That(manifest.Webapp!.BundlePath, Is.EqualTo("webapp/main.js"));
                Assert.That(manifest.Props!["enabled"].GetBoolean(), Is.True);
                Assert.That(manifest.Props["count"].GetInt32(), Is.EqualTo(3));
                Assert.That(manifest.Props["nested"].GetProperty("value").GetString(), Is.EqualTo("text"));
                Assert.That(manifest.SettingsSchema!.Header, Is.EqualTo("header"));
                Assert.That(manifest.SettingsSchema.Footer, Is.EqualTo("footer"));
                Assert.That(manifest.SettingsSchema.Settings, Is.Empty);
            });
            PluginSettingsSection section = manifest.SettingsSchema!.Sections!.Single();
            Assert.That(section.Key, Is.EqualTo("section"));
            Assert.That(section.Title, Is.EqualTo("Title"));
            Assert.That(section.Subtitle, Is.EqualTo("Subtitle"));
            Assert.That(section.Header, Is.EqualTo("section header"));
            Assert.That(section.Footer, Is.EqualTo("section footer"));
            Assert.That(section.IsCustom && section.Fallback, Is.True);
            PluginSetting setting = section.Settings!.Single();
            Assert.That(setting.Key, Is.EqualTo("setting"));
            Assert.That(setting.DisplayName, Is.EqualTo("Setting"));
            Assert.That(setting.Type, Is.EqualTo("dropdown"));
            Assert.That(setting.HelpText, Is.EqualTo("help"));
            Assert.That(setting.RegenerateHelpText, Is.EqualTo("regenerate"));
            Assert.That(setting.Placeholder, Is.EqualTo("placeholder"));
            Assert.That(setting.Hosting, Is.EqualTo("cloud"));
            Assert.That(setting.IsSecret, Is.True);
            Assert.That(setting.Default.GetProperty("value")[1].GetBoolean(), Is.True);
            Assert.That(setting.Options!.Single().DisplayName, Is.EqualTo("Option"));
            Assert.That(setting.Options!.Single().Value, Is.EqualTo("option"));
            PluginManifest inactive = response.Inactive.Single();
            Assert.That(inactive.Id, Is.EqualTo("inactive"));
            Assert.That(inactive.Server, Is.Null);
            Assert.That(inactive.Webapp, Is.Null);
            Assert.That(inactive.SettingsSchema, Is.Null);
            Assert.That(inactive.Props, Is.Null);
        }

        [Test]
        public async Task WebappMetadata_PreservesServerOmission()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "[{\"id\":\"plugin\",\"name\":\"Plugin\",\"version\":\"1\",\"webapp\":{\"bundle_path\":\"/static/plugin/hash.js\"}}]")));
            using MattermostClient client = ApiTestHttp.Client(http);
            PluginManifest manifest = (await client.GetWebappPluginsAsync()).Single();
            Assert.That(manifest.Webapp!.BundlePath, Is.EqualTo("/static/plugin/hash.js"));
            Assert.That(manifest.Server, Is.Null);
            Assert.That(manifest.SettingsSchema, Is.Null);
        }

        [TestCase("true", JsonValueKind.True)]
        [TestCase("9007199254740993", JsonValueKind.Number)]
        [TestCase("\"text\"", JsonValueKind.String)]
        [TestCase("[1,true]", JsonValueKind.Array)]
        [TestCase("{\"nested\":{\"enabled\":true}}", JsonValueKind.Object)]
        [TestCase("null", JsonValueKind.Null)]
        public async Task SettingDefaults_PreserveNativeJsonTypes(string json, JsonValueKind kind)
        {
            string response = "{\"active\":[{\"id\":\"plugin\",\"settings_schema\":{\"settings\":[{\"key\":\"setting\",\"default\":"
                + json + "}]}}],\"inactive\":[]}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            PluginSetting setting = (await client.GetPluginsAsync()).Active.Single().SettingsSchema!.Settings!.Single();
            Assert.That(setting.Default.ValueKind, Is.EqualTo(kind));
            Assert.That(setting.Default.GetRawText(), Is.EqualTo(json));
            string serialized = JsonSerializer.Serialize(setting);
            using JsonDocument document = JsonDocument.Parse(serialized);
            Assert.That(document.RootElement.GetProperty("default").GetRawText(), Is.EqualTo(json));
        }

        [TestCase("false", JsonValueKind.False)]
        [TestCase("9007199254740993", JsonValueKind.Number)]
        [TestCase("\"text\"", JsonValueKind.String)]
        [TestCase("[1,true]", JsonValueKind.Array)]
        [TestCase("{\"enabled\":true}", JsonValueKind.Object)]
        [TestCase("null", JsonValueKind.Null)]
        public async Task ManifestProperties_PreserveNativeJsonTypes(string json, JsonValueKind kind)
        {
            string response = "{\"active\":[{\"id\":\"plugin\",\"props\":{\"value\":" + json + "}}],\"inactive\":[]}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            PluginManifest manifest = (await client.GetPluginsAsync()).Active.Single();
            Assert.That(manifest.Props!["value"].ValueKind, Is.EqualTo(kind));
            Assert.That(manifest.Props["value"].GetRawText(), Is.EqualTo(json));
            string serialized = JsonSerializer.Serialize(manifest);
            using JsonDocument document = JsonDocument.Parse(serialized);
            Assert.That(document.RootElement.GetProperty("props").GetProperty("value").GetRawText(), Is.EqualTo(json));
        }

        [TestCase(0, PluginState.NotRunning)]
        [TestCase(1, PluginState.Starting)]
        [TestCase(2, PluginState.Running)]
        [TestCase(3, PluginState.FailedToStart)]
        [TestCase(4, PluginState.FailedToStayRunning)]
        [TestCase(5, PluginState.Stopping)]
        [TestCase(101, (PluginState)101)]
        public async Task Statuses_PreserveNativeNumericStatesAndMetadata(int state, PluginState expected)
        {
            string json = "[{\"plugin_id\":\"plugin\",\"cluster_id\":\"node\",\"plugin_path\":\"plugins/path\",\"state\":"
                + state + ",\"error\":\"error\",\"name\":\"Plugin\",\"description\":\"description\",\"version\":\"1\"}]";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            PluginStatus status = (await client.GetPluginStatusesAsync()).Single();
            Assert.That(status.State, Is.EqualTo(expected));
            Assert.That(status.PluginId, Is.EqualTo("plugin"));
            Assert.That(status.ClusterId, Is.EqualTo("node"));
            Assert.That(status.PluginPath, Is.EqualTo("plugins/path"));
            Assert.That(status.Error, Is.EqualTo("error"));
            Assert.That(status.Name, Is.EqualTo("Plugin"));
            Assert.That(status.Description, Is.EqualTo("description"));
            Assert.That(status.Version, Is.EqualTo("1"));
        }

        [Test]
        public async Task Marketplace_IncludesCompleteNativeMetadata()
        {
            string json = "[{\"homepage_url\":\"home\",\"icon_data\":\"data\",\"download_url\":\"download\",\"release_notes_url\":\"notes\",\"labels\":[{\"name\":\"Label\",\"description\":\"description\",\"url\":\"url\",\"color\":\"color\"}],\"hosting\":\"cloud\",\"author_type\":\"community\",\"release_stage\":\"beta\",\"enterprise\":true,\"signature\":\"signature\",\"manifest\":" + ManifestJson + ",\"installed_version\":\"1.0\"}]";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MarketplacePlugin plugin = (await client.GetMarketplacePluginsAsync()).Single();
            Assert.That(plugin.HomepageUrl, Is.EqualTo("home"));
            Assert.That(plugin.IconData, Is.EqualTo("data"));
            Assert.That(plugin.DownloadUrl, Is.EqualTo("download"));
            Assert.That(plugin.ReleaseNotesUrl, Is.EqualTo("notes"));
            Assert.That(plugin.Hosting, Is.EqualTo("cloud"));
            Assert.That(plugin.AuthorType, Is.EqualTo("community"));
            Assert.That(plugin.ReleaseStage, Is.EqualTo("beta"));
            Assert.That(plugin.IsEnterprise, Is.True);
            Assert.That(plugin.Signature, Is.EqualTo("signature"));
            Assert.That(plugin.Manifest!.Id, Is.EqualTo("plugin"));
            Assert.That(plugin.InstalledVersion, Is.EqualTo("1.0"));
            MarketplaceLabel label = plugin.Labels!.Single();
            Assert.That(label.Name, Is.EqualTo("Label"));
            Assert.That(label.Description, Is.EqualTo("description"));
            Assert.That(label.Url, Is.EqualTo("url"));
            Assert.That(label.Color, Is.EqualTo("color"));
        }

        [TestCase(false, false, "?local_only=false&remote_only=false")]
        [TestCase(false, true, "?local_only=false&remote_only=true")]
        public async Task Marketplace_DefaultsAndRemoteOnly(bool localOnly, bool remoteOnly, string query)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo(query));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetMarketplacePluginsAsync(localOnly: localOnly, remoteOnly: remoteOnly);
        }

        [TestCase("", "?local_only=false&remote_only=false&filter=")]
        [TestCase("  ", "?local_only=false&remote_only=false&filter=")]
        public async Task Marketplace_AcceptsEmptySearch(string filter, string query)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo(query));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetMarketplacePluginsAsync(filter: filter);
        }

        [Test]
        public async Task Marketplace_RejectsConflictingFilters()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetMarketplacePluginsAsync(localOnly: true, remoteOnly: true));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public async Task Visited_MapsOnlyNativeStrings(string value, bool expected)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"value\":\"" + value + "\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetMarketplaceVisitedByAdminAsync(), Is.EqualTo(expected));
        }

        [TestCase("null")]
        [TestCase("{}")]
        [TestCase("{\"value\":true}")]
        [TestCase("{\"value\":null}")]
        [TestCase("{\"value\":\"invalid\"}")]
        [TestCase("{\"value\":\"\"}")]
        public async Task Visited_RejectsMalformedNativeValues(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetMarketplaceVisitedByAdminAsync());
        }

        [Test]
        public async Task Lists_NormalizeNativeNull([Values("webapp", "statuses", "marketplace")] string operation, [Values("[]", "null")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await ReadAsync(client, operation), Is.Empty);
        }

        [Test]
        public async Task Errors_PreserveNativeDetails([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"plugins unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("plugins unavailable"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task Installed_NullObjectIsRejected()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetPluginsAsync());
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => ReadAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([ValueSource(nameof(Operations))] string operation)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                entered.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = ReadAsync(client, operation, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static readonly string[] Operations = { "installed", "webapp", "statuses", "marketplace", "visited" };

        private static async Task<object> ReadAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "installed": return await client.GetPluginsAsync(token);
                case "webapp": return await client.GetWebappPluginsAsync(token);
                case "statuses": return await client.GetPluginStatusesAsync(token);
                case "marketplace": return await client.GetMarketplacePluginsAsync(" search&value ", localOnly: true, cancellationToken: token);
                case "visited": return await client.GetMarketplaceVisitedByAdminAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
