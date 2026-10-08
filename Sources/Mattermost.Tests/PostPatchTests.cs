using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PostPatchTests
    {
        [TestCase("message", "{\"message\":\"Updated text\"}")]
        [TestCase("props", "{\"props\":{\"custom_key\":\"value\"}}")]
        [TestCase("file_ids", "{\"file_ids\":[\"file-1\"]}")]
        [TestCase("is_pinned", "{\"is_pinned\":true}")]
        [TestCase("has_reactions", "{\"has_reactions\":true}")]
        public async Task PatchPost_IndividualField_OmitsAllOtherFields(string field, string expectedJson)
        {
            int requests = 0;
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/posts/post%2F1%3Fx%23y/patch"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                using JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
                using JsonDocument expected = JsonDocument.Parse(expectedJson);
                Assert.That(JsonElement.DeepEquals(body.RootElement, expected.RootElement), Is.True);
                return JsonResponse(HttpStatusCode.OK, "{\"id\":\"post-1\",\"channel_id\":\"channel-1\",\"message\":\"Updated text\",\"is_pinned\":true}");
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            Post post = await PatchFieldAsync(api, " post/1?x#y ", field);

            Assert.That(requests, Is.EqualTo(1));
            Assert.That(post.Id, Is.EqualTo("post-1"));
            Assert.That(post.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(post.Text, Is.EqualTo("Updated text"));
            Assert.That(post.IsPinned, Is.True);
        }

        [Test]
        public async Task PatchPost_AllFields_SendsTheirValues()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                using JsonDocument expected = JsonDocument.Parse("{\"message\":\"**Updated**\",\"props\":{\"custom_key\":\"value\"},\"file_ids\":[\"file-1\",\"file-2\"],\"is_pinned\":true,\"has_reactions\":true}");
                Assert.That(JsonElement.DeepEquals(body.RootElement, expected.RootElement), Is.True);
                return JsonResponse(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = CreateClient(http);

            await client.PatchPostAsync("post-1", "**Updated**",
                new Dictionary<string, object> { ["custom_key"] = "value" },
                new[] { "file-1", "file-2" }, isPinned: true, hasReactions: true);
        }

        [Test]
        public async Task PatchPost_NullParameters_SendsEmptyObject()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{}"));
                return JsonResponse(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = CreateClient(http);

            await client.PatchPostAsync("post-1", text: null, props: null, fileIds: null,
                isPinned: null, hasReactions: null);
        }

        [Test]
        public async Task PatchPost_EmptyValuesAndFalse_AreNotOmitted()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                using JsonDocument expected = JsonDocument.Parse("{\"message\":\"\",\"props\":{},\"file_ids\":[],\"is_pinned\":false,\"has_reactions\":false}");
                Assert.That(JsonElement.DeepEquals(body.RootElement, expected.RootElement), Is.True);
                return JsonResponse(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = CreateClient(http);

            await client.PatchPostAsync("post-1", string.Empty,
                new Dictionary<string, object>(), Array.Empty<string>(), isPinned: false, hasReactions: false);
        }

        [Test]
        public async Task PatchPost_MaximumTextLength_IsAllowed()
        {
            string text = new string('a', MattermostApiLimits.MaxPostMessageLength);
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo(text));
                return JsonResponse(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = CreateClient(http);

            await client.PatchPostAsync("post-1", text);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void PatchPost_BlankIdentifier_DoesNotSendRequests(string? postId)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            ArgumentException exception = Assert.Throws<ArgumentException>(() => client.PatchPostAsync(postId!))!;

            Assert.That(exception.ParamName, Is.EqualTo("postId"));
        }

        [Test]
        public void PatchPost_OversizedText_DoesNotSendRequests()
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.PatchPostAsync("post-1", new string('a', MattermostApiLimits.MaxPostMessageLength + 1)))!;

            Assert.That(exception.ParamName, Is.EqualTo("text"));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task PatchPost_HttpFailure_IsPropagated(HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Cannot edit post\"}")));
            using MattermostClient client = CreateClient(http);

            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() =>
                client.PatchPostAsync("post-1", text: "Updated text")))!;

            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Cannot edit post"));
            Assert.That(exception.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(exception.RequestUri, Is.EqualTo("https://mattermost.example/chat/api/v4/posts/post-1/patch"));
        }

        [Test]
        public void PatchPost_CancelledAndDisposedCalls_DoNotSendRequests()
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() => client.PatchPostAsync("post-1", isPinned: false, cancellationToken: cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.PatchPostAsync("post-1", isPinned: false));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task PatchPost_Cancellation_StopsAuthenticationOrPendingRequest(bool duringAuthentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("The request must be cancelled.");
            }));
            using MattermostClient client = CreateClient(http);

            Task<Post> pending = client.PatchPostAsync("post-1", isPinned: false, cancellationToken: cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();

            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task<Post> PatchFieldAsync(IMattermostClient client, string postId, string field)
        {
            switch (field)
            {
                case "message": return client.PatchPostAsync(postId, text: "Updated text");
                case "props": return client.PatchPostAsync(postId, props: new Dictionary<string, object> { ["custom_key"] = "value" });
                case "file_ids": return client.PatchPostAsync(postId, fileIds: new[] { "file-1" });
                case "is_pinned": return client.PatchPostAsync(postId, isPinned: true);
                case "has_reactions": return client.PatchPostAsync(postId, hasReactions: true);
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
        }

        private static MattermostClient CreateClient(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);

        private static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
