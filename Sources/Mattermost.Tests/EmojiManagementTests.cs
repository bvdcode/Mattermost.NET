using Mattermost.Exceptions;
using Mattermost.Models.Emojis;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class EmojiManagementTests
    {
        private const string EmojiJson = "{\"id\":\"emoji-1\",\"name\":\"custom_emoji\",\"creator_id\":\"user-1\",\"create_at\":4200000000123}";

        [Test]
        public async Task Creation_UsesMultipartAndAuthenticatedCreator()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/emoji"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("multipart/form-data"));
                MultipartFormDataContent form = (MultipartFormDataContent)request.Content;
                HttpContent metadata = form.Single(part => part.Headers.ContentDisposition!.Name!.Trim('"') == "emoji");
                using JsonDocument json = JsonDocument.Parse(await metadata.ReadAsStringAsync(token));
                Assert.That(json.RootElement.GetProperty("name").GetString(), Is.EqualTo("custom_emoji"));
                Assert.That(json.RootElement.GetProperty("creator_id").GetString(), Is.EqualTo("user-1"));
                Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
                HttpContent image = form.Single(part => part.Headers.ContentDisposition!.Name!.Trim('"') == "image");
                Assert.That(image.Headers.ContentDisposition!.FileName!.Trim('"'), Is.EqualTo("emoji.png"));
                Assert.That(await image.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                return ApiTestHttp.Json(HttpStatusCode.Created, EmojiJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Emoji emoji = await client.CreateEmojiAsync(" :custom_emoji: ", new byte[] { 1, 2, 3 }, " emoji.png ");
            Assert.That(emoji.CreatedAt, Is.EqualTo(4200000000123));
        }

        [Test]
        public async Task Creation_CapturesImageBeforeAuthentication()
        {
            byte[] image = { 1, 2, 3 };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    image[0] = 9;
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                MultipartFormDataContent form = (MultipartFormDataContent)request.Content!;
                HttpContent part = form.Single(item => item.Headers.ContentDisposition!.Name!.Trim('"') == "image");
                Assert.That(await part.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                return ApiTestHttp.Json(HttpStatusCode.OK, EmojiJson);
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CreateEmojiAsync("emoji", image, "emoji.png");
        }

        [Test]
        public async Task Deletion_UsesEscapedIdentifierWithoutBody()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/emoji/emoji%2F1%3F%23"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.DeleteEmojiAsync(" emoji/1?# ");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidIdentifiers_AreRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateEmojiAsync(value!, new byte[] { 1 }, "emoji.png"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteEmojiAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateEmojiAsync("emoji", new byte[] { 1 }, value!));
        }

        [Test]
        public async Task EmptyOrMissingImage_IsRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateEmojiAsync("emoji", null!, "emoji.png"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateEmojiAsync("emoji", Array.Empty<byte>(), "emoji.png"));
        }

        [Test]
        public async Task Errors_PreserveServerDetails([Values(false, true)] bool create,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, create));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected([Values(false, true)] bool create, [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, create));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool create)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, create, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, create));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values(false, true)] bool create)
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
            Task pending = InvokeAsync(client, create, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, bool create, CancellationToken token = default)
        {
            if (create) { return client.CreateEmojiAsync("emoji", new byte[] { 1 }, "emoji.png", token); }
            return client.DeleteEmojiAsync("emoji", token);
        }
    }
}
