using Mattermost.Exceptions;
using System.Net;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ClientInterfaceTests
    {
        private static readonly string[] Groups = { "users", "posts", "channels", "teams", "files", "integrations", "emojis", "system" };

        private static IEnumerable<TestCaseData> ReadCases()
        {
            foreach (string group in Groups)
            {
                yield return new TestCaseData(group, false);
                yield return new TestCaseData(group, true);
            }
        }

        [TestCaseSource(nameof(ReadCases))]
        public async Task AggregateAndNarrowContracts_UseTheSameRequestAndResponse(string group, bool narrow)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(ReadRoute(group)));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"record\",\"busy\":true,\"expires\":100}"));
            });
            using MattermostClient implementation = ApiTestHttp.Client(http);
            IMattermostClient client = implementation;
            string result = await ReadAsync(client, group, narrow);
            Assert.That(result, Is.EqualTo(group == "system" ? bool.TrueString : "record"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void AllContracts_ReferToTheOriginalClientInstance()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient implementation = ApiTestHttp.Client(http);
            IMattermostClient client = implementation;
            IMattermostUsersClient users = client;
            IMattermostPostsClient posts = client;
            IMattermostChannelsClient channels = client;
            IMattermostTeamsClient teams = client;
            IMattermostFilesClient files = client;
            IMattermostIntegrationsClient integrations = client;
            IMattermostEmojisClient emojis = client;
            IMattermostSystemClient system = client;
            Assert.Multiple(() =>
            {
                Assert.That(users, Is.SameAs(implementation));
                Assert.That(posts, Is.SameAs(implementation));
                Assert.That(channels, Is.SameAs(implementation));
                Assert.That(teams, Is.SameAs(implementation));
                Assert.That(files, Is.SameAs(implementation));
                Assert.That(integrations, Is.SameAs(implementation));
                Assert.That(emojis, Is.SameAs(implementation));
                Assert.That(system, Is.SameAs(implementation));
                Assert.That(client.IsConnected, Is.False);
                Assert.That(client.ServerAddress, Is.EqualTo(new Uri("https://mattermost.example/chat")));
            });
        }

        [TestCaseSource(nameof(Groups))]
        public async Task NarrowContracts_PreserveNativeErrors(string group)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Forbidden,
                "{\"id\":\"permission_denied\",\"message\":\"Forbidden\",\"status_code\":403}")));
            using MattermostClient implementation = ApiTestHttp.Client(http);
            IMattermostClient client = implementation;
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(async () => await ReadAsync(client, group, true));
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [TestCaseSource(nameof(Groups))]
        public async Task NarrowContracts_PreserveDisposalChecks(string group)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient implementation = ApiTestHttp.Client(http);
            IMattermostClient client = implementation;
            implementation.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ReadAsync(client, group, true));
        }

        [TestCaseSource(nameof(Groups))]
        public async Task NarrowContracts_PreserveCancellationBeforeRequests(string group)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient implementation = ApiTestHttp.Client(http);
            IMattermostClient client = implementation;
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await ReadCancelableAsync(client, group, cancellation.Token));
        }

        private static string ReadRoute(string group)
        {
            switch (group)
            {
                case "users": return "/chat/api/v4/users/record";
                case "posts": return "/chat/api/v4/posts/record";
                case "channels": return "/chat/api/v4/channels/record";
                case "teams": return "/chat/api/v4/teams/record";
                case "files": return "/chat/api/v4/files/record/info";
                case "integrations": return "/chat/api/v4/commands/record";
                case "emojis": return "/chat/api/v4/emoji/record";
                case "system": return "/chat/api/v4/server_busy";
                default: throw new ArgumentOutOfRangeException(nameof(group));
            }
        }

        private static async Task<string> ReadAsync(IMattermostClient client, string group, bool narrow)
        {
            switch (group)
            {
                case "users":
                    IMattermostUsersClient users = client;
                    return (await (narrow ? users.GetUserAsync("record") : client.GetUserAsync("record"))).Id;
                case "posts":
                    IMattermostPostsClient posts = client;
                    return (await (narrow ? posts.GetPostAsync("record") : client.GetPostAsync("record"))).Id;
                case "channels":
                    IMattermostChannelsClient channels = client;
                    return (await (narrow ? channels.GetChannelAsync("record") : client.GetChannelAsync("record"))).Id;
                case "teams":
                    IMattermostTeamsClient teams = client;
                    return (await (narrow ? teams.GetTeamAsync("record") : client.GetTeamAsync("record"))).Id;
                case "files":
                    IMattermostFilesClient files = client;
                    return (await (narrow ? files.GetFileDetailsAsync("record") : client.GetFileDetailsAsync("record"))).Id;
                case "integrations":
                    IMattermostIntegrationsClient integrations = client;
                    return (await (narrow ? integrations.GetCommandAsync("record") : client.GetCommandAsync("record"))).Id;
                case "emojis":
                    IMattermostEmojisClient emojis = client;
                    return (await (narrow ? emojis.GetEmojiAsync("record") : client.GetEmojiAsync("record"))).Id;
                case "system":
                    IMattermostSystemClient system = client;
                    return (await (narrow ? system.GetServerBusyAsync() : client.GetServerBusyAsync())).IsBusy.ToString();
                default: throw new ArgumentOutOfRangeException(nameof(group));
            }
        }

        private static Task ReadCancelableAsync(IMattermostClient client, string group, CancellationToken token)
        {
            switch (group)
            {
                case "users":
                    IMattermostUsersClient users = client;
                    return users.GetUsersByUsernamesAsync(new[] { "record" }, token);
                case "posts":
                    IMattermostPostsClient posts = client;
                    return posts.GetPostsByIdsAsync(new[] { "record" }, token);
                case "channels":
                    IMattermostChannelsClient channels = client;
                    return channels.GetChannelStatsAsync("record", cancellationToken: token);
                case "teams":
                    IMattermostTeamsClient teams = client;
                    return teams.GetTeamMembersAsync("record", cancellationToken: token);
                case "files":
                    IMattermostFilesClient files = client;
                    return files.GetFilePreviewAsync("record", token);
                case "integrations":
                    IMattermostIntegrationsClient integrations = client;
                    return integrations.GetCommandAsync("record", token);
                case "emojis":
                    IMattermostEmojisClient emojis = client;
                    return emojis.GetEmojiAsync("record", token);
                case "system":
                    IMattermostSystemClient system = client;
                    return system.GetServerBusyAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(group));
            }
        }
    }
}
