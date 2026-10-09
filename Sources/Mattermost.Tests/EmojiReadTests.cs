using Mattermost.Models.Emojis;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class EmojiReadTests
    {
        [Test]
        [SetCulture("ar-SA")]
        public async Task GetEmojis_SendsPagingAndOptionalSorting(
            [Values(0, 2)] int page, [Values(1, 60, 200)] int perPage, [Values(false, true)] bool sortByName)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertGet(request);
                string expected = FormattableString.Invariant($"/chat/api/v4/emoji?page={page}&per_page={perPage}");
                if (sortByName)
                {
                    expected += "&sort=name";
                }
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(expected));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + EmojiJson() + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;
            AssertEmoji((await api.GetEmojisAsync(page, perPage, sortByName)).Single());
        }

        [Test]
        public async Task GetEmojis_UsesDefaultPagingWithoutSorting()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertGet(request);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/emoji?page=0&per_page=60"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = CreateClient(http);
            Assert.That(await client.GetEmojisAsync(), Is.Empty);
        }

        [Test]
        public async Task GetEmoji_EscapesIdentifierAndPreservesMetadata()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertGet(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/emoji/emoji%2F1%3F%23"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, EmojiJson()));
            });
            using MattermostClient client = CreateClient(http);
            AssertEmoji(await client.GetEmojiAsync(" emoji/1?# "));
        }

        [TestCase(" rocket/1?# ")]
        [TestCase(" :rocket/1?#: ")]
        public async Task GetEmojiByName_TrimsColonsAndEscapesName(string name)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertGet(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/emoji/name/rocket%2F1%3F%23"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, EmojiJson()));
            });
            using MattermostClient client = CreateClient(http);
            AssertEmoji(await client.GetEmojiByNameAsync(name));
        }

        [Test]
        public async Task GetEmojisByNames_SendsRawArrayAndEnumeratesOnce()
        {
            int enumerations = 0;
            IEnumerable<string> Names()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " :rocket: ";
                yield return "Rocket";
                yield return "rocket";
                yield return "missing";
            }
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/emoji/names"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                string[]? names = JsonSerializer.Deserialize<string[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(names, Is.EqualTo(new[] { "rocket", "Rocket", "missing" }));
                return JsonResponse(HttpStatusCode.OK, "[" + EmojiJson() + "]");
            });
            using MattermostClient client = CreateClient(http);
            AssertEmoji((await client.GetEmojisByNamesAsync(Names())).Single());
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task GetEmojisByNames_Allows200DistinctNamesAndDuplicateInputs(bool addDuplicates)
        {
            List<string> names = Enumerable.Range(0, 200).Select(index => "emoji_" + index).ToList();
            if (addDuplicates)
            {
                names.AddRange(names.Select(name => " :" + name + ": ").ToArray());
            }
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                string[]? sent = JsonSerializer.Deserialize<string[]>(await request.Content!.ReadAsStringAsync(token));
                Assert.That(sent, Has.Length.EqualTo(200));
                return JsonResponse(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = CreateClient(http);
            Assert.That(await client.GetEmojisByNamesAsync(names), Is.Empty);
        }

        [Test]
        public async Task SearchEmojis_SendsTermAndBooleanPrefixFilter([Values(false, true)] bool prefixOnly)
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/emoji/search"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                using JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
                Assert.That(body.RootElement.GetProperty("term").GetString(), Is.EqualTo("Rocket"));
                Assert.That(body.RootElement.GetProperty("prefix_only").GetBoolean(), Is.EqualTo(prefixOnly));
                return JsonResponse(HttpStatusCode.OK, "[" + EmojiJson() + "]");
            });
            using MattermostClient client = CreateClient(http);
            AssertEmoji((await client.SearchEmojisAsync(" :Rocket: ", prefixOnly)).Single());
        }

        [Test]
        public async Task SearchEmojis_DefaultsToSubstringSearch()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("prefix_only").GetBoolean(), Is.False);
                return JsonResponse(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = CreateClient(http);
            Assert.That(await client.SearchEmojisAsync("rocket"), Is.Empty);
        }

        [Test]
        public async Task AutocompleteEmojis_EscapesQueryAndTrimsColons()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertGet(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/emoji/autocomplete?name=rocket%2B%26%3F%23"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + EmojiJson() + "]"));
            });
            using MattermostClient client = CreateClient(http);
            AssertEmoji((await client.AutocompleteEmojisAsync(" :rocket+&?#: ")).Single());
        }

        [TestCase("list")]
        [TestCase("batch")]
        [TestCase("search")]
        [TestCase("autocomplete")]
        public async Task EmptyCollections_RemainEmpty(string operation)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]")));
            using MattermostClient client = CreateClient(http);
            Assert.That(await InvokeCollectionAsync(client, operation), Is.Empty);
        }

        private static string EmojiJson() => "{\"id\":\"emoji-1\",\"name\":\"rocket\",\"creator_id\":\"user-1\",\"create_at\":2147483648,\"update_at\":9223372036854775807,\"delete_at\":0}";

        private static void AssertEmoji(Emoji emoji)
        {
            Assert.That(emoji.Id, Is.EqualTo("emoji-1"));
            Assert.That(emoji.Name, Is.EqualTo("rocket"));
            Assert.That(emoji.CreatorId, Is.EqualTo("user-1"));
            Assert.That(emoji.CreatedAt, Is.EqualTo(2147483648L));
            Assert.That(emoji.UpdatedAt, Is.EqualTo(long.MaxValue));
            Assert.That(emoji.DeletedAt, Is.Zero);
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

        private static void AssertGet(HttpRequestMessage request)
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Content, Is.Null);
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
