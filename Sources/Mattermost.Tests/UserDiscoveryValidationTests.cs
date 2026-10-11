using System.Globalization;
using System.Net;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserDiscoveryValidationTests
    {
        [Test]
        public async Task UserIdentifiers_RejectBlankBeforeHttp([Values("members", "sessions", "audits")] string operation,
            [Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "members": await Assert.ThrowsAsync<ArgumentException>(() => client.GetUserChannelMembersAsync(value!)); break;
                case "sessions": await Assert.ThrowsAsync<ArgumentException>(() => client.GetUserSessionsAsync(value!)); break;
                case "audits": await Assert.ThrowsAsync<ArgumentException>(() => client.GetUserAuditsAsync(value!)); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task Autocomplete_RejectsInvalidArgumentsBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.AutocompleteUsersAsync(null!));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.AutocompleteUsersAsync("", limit: 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.AutocompleteUsersAsync("", limit: -1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AutocompleteUsersAsync("", teamId: " "));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AutocompleteUsersAsync("", channelId: " "));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AutocompleteUsersAsync("", channelId: "channel"));
        }

        [Test]
        public async Task GroupLookup_RejectsNullEmptyAndBlankIdsBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetUsersByGroupChannelIdsAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUsersByGroupChannelIdsAsync(Array.Empty<string>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUsersByGroupChannelIdsAsync(new[] { "group", " " }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUsersByGroupChannelIdsAsync(Enumerable.Range(0, 51).Select(index => "group-" + index)));
        }

        [Test]
        public async Task GroupLookup_AcceptsNativeBatchLimit()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUsersByGroupChannelIdsAsync(Enumerable.Range(0, 50).Select(index => "group-" + index));
        }

        [TestCase("members", -1, 60)]
        [TestCase("members", 0, 0)]
        [TestCase("audits", -1, 60)]
        [TestCase("audits", 0, -1)]
        public async Task Paging_RejectsInvalidValuesBeforeHttp(string operation, int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "members": await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserChannelMembersAsync("me", page, perPage)); break;
                case "audits": await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserAuditsAsync("me", page, perPage)); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task Paging_UsesInvariantQuery([Values("members", "audits")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=12&per_page=17"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
                switch (operation)
                {
                    case "members": await client.GetUserChannelMembersAsync("me", 12, 17); break;
                    case "audits": await client.GetUserAuditsAsync("me", 12, 17); break;
                    default: throw new ArgumentOutOfRangeException(nameof(operation));
                }
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public async Task GroupLookup_SnapshotsBeforeAuthentication()
        {
            List<string> ids = new List<string> { "group-1" };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    ids[0] = "changed";
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("[\"group-1\"]"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUsersByGroupChannelIdsAsync(ids);
        }
    }
}
