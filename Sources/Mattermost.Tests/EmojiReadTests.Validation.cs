using Mattermost.Exceptions;
using Mattermost.Models.Emojis;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class EmojiReadTests
    {
        [TestCase(-1, 60, "page")]
        [TestCase(0, 0, "perPage")]
        [TestCase(0, -1, "perPage")]
        [TestCase(0, 201, "perPage")]
        public void InvalidPaging_DoesNotSendRequests(int page, int perPage, string parameterName)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetEmojisAsync(page, perPage))!.ParamName,
                Is.EqualTo(parameterName));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankIdentifier_DoesNotSendRequests(string? identifier)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetEmojiAsync(identifier!))!.ParamName, Is.EqualTo("emojiId"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        [TestCase("::")]
        [TestCase(" : : ")]
        public void BlankNamesAndTerms_DoNotSendRequests(string? name)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetEmojiByNameAsync(name!))!.ParamName, Is.EqualTo("emojiName"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.SearchEmojisAsync(name!))!.ParamName, Is.EqualTo("term"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.AutocompleteEmojisAsync(name!))!.ParamName, Is.EqualTo("name"));
        }

        [TestCaseSource(nameof(InvalidNames))]
        public void InvalidBatch_DoesNotSendRequests(string[]? names)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Catch<ArgumentException>(() => client.GetEmojisByNamesAsync(names!))!.ParamName,
                Is.EqualTo("emojiNames"));
        }

        private static IEnumerable<TestCaseData> InvalidNames()
        {
            yield return new TestCaseData(new object?[] { null });
            yield return new TestCaseData(new object[] { Array.Empty<string>() });
            yield return new TestCaseData(new object[] { new string[] { null! } });
            yield return new TestCaseData(new object[] { new[] { "first", "" } });
            yield return new TestCaseData(new object[] { new[] { " \t" } });
            yield return new TestCaseData(new object[] { new[] { ": :" } });
            yield return new TestCaseData(new object[] { Enumerable.Range(0, 201).Select(index => "emoji_" + index).ToArray() });
        }

        [Test]
        public async Task HttpFailures_ArePropagated(
            [Values("list", "id", "name", "batch", "search", "autocomplete")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound, HttpStatusCode.NotImplemented, HttpStatusCode.ServiceUnavailable)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Unavailable\"}")));
            using MattermostClient client = CreateClient(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;
            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Unavailable"));
        }

        [Test]
        public async Task InvalidResponses_AreRejected(
            [Values("list", "id", "name", "batch", "search", "autocomplete")] string operation,
            [Values("null", "invalid JSON", "42")] string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("list", "{}")]
        [TestCase("id", "[]")]
        [TestCase("name", "[]")]
        [TestCase("batch", "{}")]
        [TestCase("search", "{}")]
        [TestCase("autocomplete", "{}")]
        public async Task WrongResponseShapes_AreRejected(string operation, string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("list")]
        [TestCase("id")]
        [TestCase("name")]
        [TestCase("batch")]
        [TestCase("search")]
        [TestCase("autocomplete")]
        public async Task CancelledAndDisposedCalls_DoNotSendRequests(string operation)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_ReachesAuthenticationAndRequest(
            [Values("list", "id", "name", "batch", "search", "autocomplete")] string operation,
            [Values(false, true)] bool duringAuthentication)
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
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
            Task pending = InvokeAsync(client, operation, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "id": return client.GetEmojiAsync("emoji-1", token);
                case "name": return client.GetEmojiByNameAsync("rocket", token);
                case "list":
                case "batch":
                case "search":
                case "autocomplete": return InvokeCollectionAsync(client, operation, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task<IList<Emoji>> InvokeCollectionAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetEmojisAsync(cancellationToken: token);
                case "batch": return client.GetEmojisByNamesAsync(new[] { "rocket", "missing" }, token);
                case "search": return client.SearchEmojisAsync("rocket", cancellationToken: token);
                case "autocomplete": return client.AutocompleteEmojisAsync("rock", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
