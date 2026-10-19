using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PostMessageLengthTests
    {
        [Test]
        public void MaximumLength_MatchesMattermost12()
        {
            Assert.That(MattermostApiLimits.MaxPostMessageLength, Is.EqualTo(262144));
        }

        [Test]
        public async Task Write_AcceptsTextWithinLimit(
            [Values("create", "create-raw", "update", "update-raw", "patch", "replace", "ephemeral")] string operation,
            [Values(0, 16384, 262144)] int codePoints,
            [Values("ascii", "emoji", "combining", "mixed")] string content)
        {
            string text = CreateText(content, codePoints);
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                requests++;
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement post = body.RootElement;
                if (operation == "ephemeral")
                {
                    post = post.GetProperty("post");
                }
                Assert.That(post.GetProperty("message").GetString(), Is.EqualTo(text));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"post-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);

            Post result = await WriteAsync(client, operation, text);

            Assert.That(result.Id, Is.EqualTo("post-1"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase("create", "message")]
        [TestCase("create-raw", "message")]
        [TestCase("update", "newText")]
        [TestCase("update-raw", "newText")]
        [TestCase("patch", "text")]
        [TestCase("replace", "post")]
        [TestCase("ephemeral", "message")]
        public async Task Write_RejectsTextAboveLimitBeforeHttp(string operation, string parameterName)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string content in new[] { "ascii", "emoji", "combining", "mixed" })
            {
                string text = CreateText(content, 262145);

                ArgumentOutOfRangeException? error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => WriteAsync(client, operation, text));

                Assert.That(error!.ParamName, Is.EqualTo(parameterName));
                Assert.That(error.Message, Does.Contain("262145 > 262144"));
            }
        }

        [Test]
        public async Task Write_PreservesOlderServerLengthError(
            [Values("create", "create-raw", "update", "update-raw", "patch", "replace", "ephemeral")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(
                HttpStatusCode.BadRequest, "{\"message\":\"Message exceeds the server limit of 16383 characters.\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);

            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(
                () => WriteAsync(client, operation, new string('a', 16384)));

            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Message, Is.EqualTo("Message exceeds the server limit of 16383 characters."));
        }

        private static string CreateText(string content, int codePoints)
        {
            if (codePoints == 0)
            {
                return string.Empty;
            }
            switch (content)
            {
                case "ascii":
                    return new string('a', codePoints);
                case "emoji":
                    return string.Concat(Enumerable.Repeat("\U0001F680", codePoints));
                case "combining":
                    string text = string.Concat(Enumerable.Repeat("e\u0301", codePoints / 2));
                    if (codePoints % 2 != 0)
                    {
                        text += "e";
                    }
                    return text;
                case "mixed":
                    return new string('a', codePoints - 1) + "\U0001F680";
                default:
                    throw new ArgumentOutOfRangeException(nameof(content));
            }
        }

        private static Task<Post> WriteAsync(MattermostClient client, string operation, string text)
        {
            switch (operation)
            {
                case "create": return client.CreatePostAsync("channel-1", text);
                case "create-raw": return client.CreatePostWithRawPropsAsync("channel-1", text);
                case "update": return client.UpdatePostAsync("post-1", text);
                case "update-raw": return client.UpdatePostWithRawPropsAsync("post-1", text);
                case "patch": return client.PatchPostAsync("post-1", text);
                case "replace": return client.ReplacePostAsync(new Post { Id = "post-1", Text = text });
                case "ephemeral": return client.CreateEphemeralPostAsync("user-1", "channel-1", text);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
