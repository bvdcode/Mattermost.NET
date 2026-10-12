using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PostReplacementTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task ReplacePost_SendsOnlyEditableFieldsAndBothFlags(bool flag)
        {
            Post input = new Post
            {
                Id = " post/1?x#y ", Text = "  replacement  ", IsPinned = flag, HasReactions = flag,
                FileIdentifiers = new[] { " file-1 ", "file-2" },
                RawProps = new Dictionary<string, object> { ["custom"] = "value" },
                ChannelId = "channel-1", UserId = "user-1", RootId = "root-1", Type = "system_join_channel",
                CreatedAt = 123, UpdatedAt = 234, EditedAt = 345, DeletedAt = 456, Hashtags = "#old",
                OriginalId = "original-1", PendingPostId = "pending-1", ReplyCount = 4, LastReplyAt = 5
            };
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/posts/post%2F1%3Fx%23y"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                using JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(item => item.Name), Is.EquivalentTo(new[] { "id", "message", "is_pinned", "has_reactions", "props", "file_ids" }));
                Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("post/1?x#y"));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo(input.Text));
                Assert.That(body.RootElement.GetProperty("is_pinned").GetBoolean(), Is.EqualTo(flag));
                Assert.That(body.RootElement.GetProperty("has_reactions").GetBoolean(), Is.EqualTo(flag));
                Assert.That(body.RootElement.GetProperty("props").GetProperty("custom").GetString(), Is.EqualTo("value"));
                Assert.That(body.RootElement.GetProperty("file_ids").EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "file-1", "file-2" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\",\"message\":\"replacement\",\"is_pinned\":true,\"has_reactions\":true}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IMattermostPostsClient api = client;

            Post result = await api.ReplacePostAsync(input);

            Assert.That(requests, Is.EqualTo(1));
            Assert.That(result.Id, Is.EqualTo("post-1"));
            Assert.That(result.Text, Is.EqualTo("replacement"));
            Assert.That(result.IsPinned && result.HasReactions, Is.True);
            Assert.That(input.FileIdentifiers[0], Is.EqualTo(" file-1 "));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task ReplacePost_NullAndEmptyCollectionsRemainDistinct(bool nullCollections)
        {
            Post input = new Post { Id = "post-1", Text = string.Empty };
            if (nullCollections) { input.FileIdentifiers = null!; input.RawProps = null!; }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                string expectedJson = "{\"id\":\"post-1\",\"message\":\"\",\"is_pinned\":false,\"has_reactions\":false,\"props\":{},\"file_ids\":[]}";
                if (nullCollections) { expectedJson = "{\"id\":\"post-1\",\"message\":\"\",\"is_pinned\":false,\"has_reactions\":false,\"props\":null,\"file_ids\":null}"; }
                using JsonDocument expected = JsonDocument.Parse(expectedJson);
                Assert.That(JsonElement.DeepEquals(body.RootElement, expected.RootElement), Is.True);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ReplacePostAsync(input);
        }

        [Test]
        public async Task ReplacePost_SnapshotsMutableInputBeforeAuthentication()
        {
            Dictionary<string, object> nested = new Dictionary<string, object> { ["value"] = "original" };
            Post input = new Post
            {
                Id = "post-1", Text = "original", IsPinned = true, HasReactions = true,
                FileIdentifiers = new List<string> { "file-1" },
                RawProps = new Dictionary<string, object> { ["nested"] = nested }
            };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    input.Id = "changed"; input.Text = "changed"; input.IsPinned = false; input.HasReactions = false;
                    input.FileIdentifiers.Clear(); nested["value"] = "changed"; input.RawProps.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                Assert.That(request.RequestUri.AbsolutePath, Does.EndWith("/posts/post-1"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                using JsonDocument expected = JsonDocument.Parse("{\"id\":\"post-1\",\"message\":\"original\",\"is_pinned\":true,\"has_reactions\":true,\"props\":{\"nested\":{\"value\":\"original\"}},\"file_ids\":[\"file-1\"]}");
                Assert.That(JsonElement.DeepEquals(body.RootElement, expected.RootElement), Is.True);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ReplacePostAsync(input);
        }

        [Test]
        public async Task ReplacePost_MaximumTextLengthIsAccepted()
        {
            string text = new string('a', MattermostApiLimits.MaxPostMessageLength);
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo(text));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ReplacePostAsync(new Post { Id = "post-1", Text = text });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void ReplacePost_BlankIdentifierIsRejected(string? identifier)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.ReplacePostAsync(new Post { Id = identifier!, Text = "text" }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void ReplacePost_BlankFileIdentifierIsRejected(string? identifier)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = "text", FileIdentifiers = new[] { identifier! } }));
        }

        [Test]
        public void ReplacePost_InvalidPostAndTextAreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentNullException>(() => client.ReplacePostAsync(null!));
            Assert.Throws<ArgumentException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = null }));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = new string('a', MattermostApiLimits.MaxPostMessageLength + 1) }));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task ReplacePost_NativeFailureIsPropagated(HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot replace post\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = "text" })))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot replace post"));
            Assert.That(error.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(error.RequestUri, Does.EndWith("/posts/post-1"));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{broken")]
        public async Task ReplacePost_InvalidResponseIsRejected(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = "text" }));
        }

        [Test]
        public void ReplacePost_CancelledAndDisposedCallsSendNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = "text" }, cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.ReplacePostAsync(new Post { Id = "post-1", Text = "text" }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ReplacePost_CancellationStopsAuthenticationOrRequest(bool authentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("Expected cancellation.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task<Post> pending = client.ReplacePostAsync(new Post { Id = "post-1", Text = "text" }, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Post_DefaultSerializationMatchesPriorContract()
        {
            Post post = new Post { Id = "post-1", Text = "text", ChannelId = "channel-1", UserId = "user-1" };
            Assert.That(JsonSerializer.Serialize(post), Is.EqualTo("{\"id\":\"post-1\",\"create_at\":0,\"update_at\":0,\"edit_at\":0,\"delete_at\":0,\"is_pinned\":false,\"user_id\":\"user-1\",\"channel_id\":\"channel-1\",\"root_id\":null,\"original_id\":null,\"message\":\"text\",\"type\":null,\"hashtags\":null,\"pending_post_id\":null,\"reply_count\":0,\"last_reply_at\":0,\"file_ids\":[],\"props\":{}}"));
        }

        [TestCase("{}", false)]
        [TestCase("{\"has_reactions\":false}", false)]
        [TestCase("{\"has_reactions\":true}", true)]
        public void Post_ReactionFlagReadsAndSerializes(string json, bool expected)
        {
            Post post = JsonSerializer.Deserialize<Post>(json)!;
            Assert.That(post.HasReactions, Is.EqualTo(expected));
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(post));
            Assert.That(serialized.RootElement.TryGetProperty("has_reactions", out JsonElement flag), Is.EqualTo(expected));
            if (expected) { Assert.That(flag.GetBoolean(), Is.True); }
        }

        [TestCase("create")]
        [TestCase("create-raw")]
        [TestCase("update")]
        [TestCase("update-raw")]
        public async Task ExistingPostRequestsRemainUnchanged(string operation)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                string expected = "{\"message\":\"text\",\"props\":null}";
                HttpMethod method = HttpMethod.Put;
                string path = "/chat/api/v4/posts/post-1/patch";
                if (operation.StartsWith("create", StringComparison.Ordinal))
                {
                    expected = "{\"message\":\"text\",\"channel_id\":\"channel-1\",\"root_id\":\"\",\"metadata\":{},\"file_ids\":null,\"props\":null}";
                    method = HttpMethod.Post; path = "/chat/api/v4/posts";
                }
                Assert.That(request.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo(path));
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\",\"has_reactions\":true}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task<Post> pending;
            switch (operation)
            {
                case "create": pending = client.CreatePostAsync("channel-1", "text"); break;
                case "create-raw": pending = client.CreatePostWithRawPropsAsync("channel-1", "text"); break;
                case "update": pending = client.UpdatePostAsync("post-1", "text"); break;
                case "update-raw": pending = client.UpdatePostWithRawPropsAsync("post-1", "text"); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
            Assert.That((await pending).HasReactions, Is.True);
        }
    }
}
