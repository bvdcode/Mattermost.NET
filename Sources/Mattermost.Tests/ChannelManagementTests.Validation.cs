using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class ChannelManagementTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_AreRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in new[] { "update", "patch", "privacy", "restore", "deleted", "private", "batch", "search" })
            {
                await Assert.CatchAsync<ArgumentException>(() => InvokeAsync(client, operation, identifier: value!));
            }
        }

        [TestCase(ChannelType.Direct)]
        [TestCase(ChannelType.Group)]
        [TestCase((ChannelType)99)]
        public void UnsupportedPrivacy_IsRejected(ChannelType privacy)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.UpdateChannelPrivacyAsync("channel-1", privacy))!.ParamName, Is.EqualTo("privacy"));
        }

        [TestCase(-1, 60)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 201)]
        public void InvalidPaging_IsRejected(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetDeletedChannelsAsync("team-1", page, perPage));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetPrivateChannelsAsync("team-1", page, perPage));
        }

        [Test]
        public void InvalidArguments_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentNullException>(() => client.UpdateChannelAsync(null!));
            Assert.Throws<ArgumentNullException>(() => client.GetPublicChannelsByIdsAsync("team-1", null!));
            Assert.Throws<ArgumentException>(() => client.GetPublicChannelsByIdsAsync("team-1", Array.Empty<string>()));
            Assert.Throws<ArgumentException>(() => client.GetPublicChannelsByIdsAsync("team-1", new[] { "channel-1", "" }));
            Assert.Throws<ArgumentNullException>(() => client.SearchTeamChannelsAsync("team-1", null!));
        }

        [Test]
        public async Task Errors_ArePropagated(
            [Values("update", "patch", "privacy", "restore", "deleted", "private", "batch", "search")] string operation,
            [Values(400, 401, 403, 404, 503)] int status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json((HttpStatusCode)status, "{\"message\":\"Denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;
            Assert.That(exception.StatusCode, Is.EqualTo((HttpStatusCode)status));
        }

        [Test]
        public async Task InvalidJson_IsRejected(
            [Values("update", "patch", "privacy", "restore", "deleted", "private", "batch", "search")] string operation,
            [Values("null", "invalid", "42")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_AreRespected(
            [Values("update", "patch", "privacy", "restore", "deleted", "private", "batch", "search")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, new CancellationToken(true)));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default, string identifier = "channel-1")
        {
            switch (operation)
            {
                case "update": return client.UpdateChannelAsync(new Channel { Id = identifier }, token);
                case "patch": return client.PatchChannelAsync(identifier, header: "", cancellationToken: token);
                case "privacy": return client.UpdateChannelPrivacyAsync(identifier, ChannelType.Private, token);
                case "restore": return client.RestoreChannelAsync(identifier, token);
                case "deleted": return client.GetDeletedChannelsAsync(identifier, cancellationToken: token);
                case "private": return client.GetPrivateChannelsAsync(identifier, cancellationToken: token);
                case "batch": return client.GetPublicChannelsByIdsAsync(identifier, new[] { "channel-1" }, token);
                case "search": return client.SearchTeamChannelsAsync(identifier, "test", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
