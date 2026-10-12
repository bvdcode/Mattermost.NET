using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class EphemeralPostTests
    {
        [Test]
        public async Task Create_SendsOnlyRecipientAndWritablePostFields([Values(false, true)] bool attachments,
            [Values(false, true)] bool thread)
        {
            const string message = " Message\n  preserved whitespace ";
            PostProps? props = null;
            if (attachments)
            {
                props = new PostProps { Attachments = new List<PostPropsAttachment> { new PostPropsAttachment { Text = "Attachment" } } };
            }
            string rootId = "";
            if (thread) { rootId = " root-1 "; }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/posts/ephemeral"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[] { "user_id", "post" }));
                Assert.That(body.RootElement.GetProperty("user_id").GetString(), Is.EqualTo("recipient-1"));
                JsonElement post = body.RootElement.GetProperty("post");
                Assert.That(post.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[] { "channel_id", "message", "root_id", "props" }));
                Assert.That(post.GetProperty("channel_id").GetString(), Is.EqualTo("channel-1"));
                Assert.That(post.GetProperty("message").GetString(), Is.EqualTo(message));
                Assert.That(post.GetProperty("root_id").GetString(), Is.EqualTo(rootId.Trim()));
                if (attachments) { Assert.That(post.GetProperty("props").GetProperty("attachments")[0].GetProperty("text").GetString(), Is.EqualTo("Attachment")); }
                else { Assert.That(post.GetProperty("props").ValueKind, Is.EqualTo(JsonValueKind.Null)); }
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"id\":\"ephemeral-1\",\"channel_id\":\"channel-1\",\"user_id\":\"author-1\",\"type\":\"system_ephemeral\",\"message\":\"Ephemeral\",\"create_at\":4294967297}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Post created = await client.CreateEphemeralPostAsync(" recipient-1 ", " channel-1 ", message, rootId, props);
            Assert.That(created.Id, Is.EqualTo("ephemeral-1"));
            Assert.That(created.UserId, Is.EqualTo("author-1"));
            Assert.That(created.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(created.Type, Is.EqualTo("system_ephemeral"));
            Assert.That(created.CreatedAt, Is.EqualTo(4294967297L));
        }

        [TestCase(0)]
        [TestCase(MattermostApiLimits.MaxPostMessageLength)]
        public async Task Create_AcceptsEmptyAndMaximumLengthText(int length)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("post").GetProperty("message").GetString(), Has.Length.EqualTo(length));
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"id\":\"ephemeral-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CreateEphemeralPostAsync("recipient-1", "channel-1", new string('x', length));
        }

        [TestCase("user", null)]
        [TestCase("user", "")]
        [TestCase("user", " ")]
        [TestCase("user", " me ")]
        [TestCase("channel", null)]
        [TestCase("channel", "")]
        [TestCase("channel", " ")]
        [TestCase("message", null)]
        [TestCase("root", null)]
        public async Task Create_RejectsInvalidInputs(string field, string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            string user = "recipient-1", channel = "channel-1", message = "Message", root = "";
            switch (field)
            {
                case "user": user = value!; break;
                case "channel": channel = value!; break;
                case "message": message = value!; break;
                case "root": root = value!; break;
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
            await Assert.CatchAsync<ArgumentException>(() => client.CreateEphemeralPostAsync(user, channel, message, root));
        }

        [Test]
        public async Task Create_RejectsTextBeyondLibraryLimit()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.CreateEphemeralPostAsync("recipient-1", "channel-1",
                new string('x', MattermostApiLimits.MaxPostMessageLength + 1)));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Create_PreservesServerErrors(HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"ephemeral rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.CreateEphemeralPostAsync("recipient-1", "channel-1"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("ephemeral rejected"));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("invalid JSON")]
        public async Task Create_RejectsMalformedResponses(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Created, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.CreateEphemeralPostAsync("recipient-1", "channel-1"));
        }

        [Test]
        public async Task Create_CancellationAndDisposalRejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => client.CreateEphemeralPostAsync("recipient-1", "channel-1", cancellationToken: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.CreateEphemeralPostAsync("recipient-1", "channel-1"));
        }

        [Test]
        public async Task Create_CancellationReachesHttp()
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
            Task pending = client.CreateEphemeralPostAsync("recipient-1", "channel-1", cancellationToken: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }
    }
}
