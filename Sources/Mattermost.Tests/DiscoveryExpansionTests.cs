using Mattermost.Exceptions;
using Mattermost.Models;
using Mattermost.Models.Channels;
using Mattermost.Models.Teams;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class DiscoveryExpansionTests
    {
        [TestCase("autocomplete", "/chat/api/v4/teams/id%2F1/channels/autocomplete?name=A%26B%20%E2%9C%A8")]
        [TestCase("search", "/chat/api/v4/teams/id%2F1/channels/search_autocomplete?name=A%26B%20%E2%9C%A8")]
        [TestCase("recommended", "/chat/api/v4/teams/id%2F1/channels/recommended")]
        [TestCase("common-teams", "/chat/api/v4/channels/id%2F1/common_teams")]
        [TestCase("uploads", "/chat/api/v4/users/id%2F1/uploads")]
        public async Task Reads_UseNativeRoutesAndModels(string operation, string route)
        {
            const string json = "[{\"id\":\"item\",\"name\":\"channel\",\"team_id\":\"team\",\"display_name\":\"Display\",\"user_id\":\"user\",\"file_size\":9007199254740991,\"file_offset\":7,\"filename\":\"file.txt\",\"type\":\"attachment\"}]";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object result = await ReadAsync(client, operation, " id/1 ");
            switch (operation)
            {
                case "autocomplete":
                case "search":
                case "recommended":
                    Channel channel = ((IList<Channel>)result).Single();
                    Assert.That(channel.Id, Is.EqualTo("item"));
                    Assert.That(channel.TeamId, Is.EqualTo("team"));
                    break;
                case "common-teams":
                    Team team = ((IList<Team>)result).Single();
                    Assert.That(team.Id, Is.EqualTo("item"));
                    Assert.That(team.DisplayName, Is.EqualTo("Display"));
                    break;
                case "uploads":
                    UploadSession upload = ((IList<UploadSession>)result).Single();
                    Assert.That(upload.Id, Is.EqualTo("item"));
                    Assert.That(upload.FileSize, Is.EqualTo(9007199254740991L));
                    Assert.That(upload.FileOffset, Is.EqualTo(7));
                    Assert.That(upload.FileName, Is.EqualTo("file.txt"));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task EmptyCollections_NormalizeNativeNull(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation,
            [Values("null", "[]")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await ReadAsync(client, operation, "id"), Is.Empty);
        }

        [Test]
        public async Task BlankIdentifiers_AreRejectedBeforeHttp(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(client, operation, id!));
        }

        [Test]
        public async Task Autocomplete_RejectsNullButAllowsEmptyName([Values(false, true)] bool search)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?name="));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (search)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.AutocompleteChannelsForSearchAsync("team", null!));
                await client.AutocompleteChannelsForSearchAsync("team", " ");
            }
            else
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.AutocompleteChannelsAsync("team", null!));
                await client.AutocompleteChannelsAsync("team", " ");
            }
        }

        [Test]
        public async Task NativeErrors_PropagateDetails(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, operation, "id"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task InvalidShapes_AreRejected(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation,
            [Values("{}", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation, "id"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => ReadAsync(client, operation, "id", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ReadAsync(client, operation, "id"));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp(
            [Values("autocomplete", "search", "recommended", "common-teams", "uploads")] string operation)
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
            Task pending = ReadAsync(client, operation, "id", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static async Task<object> ReadAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "autocomplete": return await client.AutocompleteChannelsAsync(id, " A&B ✨ ", token);
                case "search": return await client.AutocompleteChannelsForSearchAsync(id, " A&B ✨ ", token);
                case "recommended": return await client.GetRecommendedChannelsAsync(id, token);
                case "common-teams": return await client.GetChannelMembersCommonTeamsAsync(id, token);
                case "uploads": return await client.GetUserUploadsAsync(id, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
