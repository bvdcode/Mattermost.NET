using Mattermost.Models.OAuth;

namespace Mattermost.Tests
{
    internal partial class OAuthAppTests
    {
        [TestCaseSource(nameof(Operations))]
        public async Task Operations_CancelBeforeAuthenticationOrRequests(string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await InvokeAsync(client, operation, cancellation.Token));
        }

        [TestCaseSource(nameof(Operations))]
        public async Task Operations_CancelAnInFlightRequest(string operation)
        {
            TaskCompletionSource<bool> entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, requestToken) =>
            {
                entered.SetResult(true);
                await Task.Delay(Timeout.Infinite, requestToken);
                throw new AssertionException("Expected request cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            Task request = InvokeAsync(client, operation, cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await request);
        }

        [TestCaseSource(nameof(Operations))]
        public async Task Operations_RejectDisposedClients(string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await InvokeAsync(client, operation));
        }

        [TestCase("get")]
        [TestCase("info")]
        [TestCase("delete")]
        [TestCase("secret")]
        [TestCase("authorized")]
        public async Task Identifiers_RejectNullEmptyAndWhitespace(string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string? id in new string?[] { null, "", " " })
            {
                await Assert.ThrowsAsync<ArgumentException>(async () =>
                {
                    switch (operation)
                    {
                        case "get": await client.GetOAuthAppAsync(id!); break;
                        case "info": await client.GetOAuthAppInfoAsync(id!); break;
                        case "delete": await client.DeleteOAuthAppAsync(id!); break;
                        case "secret": await client.RegenerateOAuthAppSecretAsync(id!); break;
                        case "authorized": await client.GetAuthorizedOAuthAppsAsync(id!); break;
                        default: throw new ArgumentOutOfRangeException(nameof(operation));
                    }
                });
            }
        }

        [TestCase("list", -1, 1)]
        [TestCase("list", 0, 0)]
        [TestCase("list", 0, -1)]
        [TestCase("authorized", -1, 1)]
        [TestCase("authorized", 0, 0)]
        [TestCase("authorized", 0, -1)]
        public async Task Pagination_RejectsInvalidValues(string operation, int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                if (operation == "list") { await client.GetOAuthAppsAsync(page, perPage); }
                else { await client.GetAuthorizedOAuthAppsAsync("me", page, perPage); }
            });
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Writes_RejectNullApplication(bool update)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                if (update) { await client.UpdateOAuthAppAsync(null!); }
                else { await client.CreateOAuthAppAsync(null!); }
            });
        }

        [TestCase("name", false)]
        [TestCase("homepage", false)]
        [TestCase("callbacks", false)]
        [TestCase("empty-callbacks", false)]
        [TestCase("callback", false)]
        [TestCase("icon", false)]
        [TestCase("name", true)]
        [TestCase("homepage", true)]
        [TestCase("callbacks", true)]
        [TestCase("empty-callbacks", true)]
        [TestCase("callback", true)]
        [TestCase("icon", true)]
        [TestCase("id", true)]
        public async Task Writes_RejectInvalidApplicationBeforeRequests(string invalidField, bool update)
        {
            OAuthApp application = Application();
            switch (invalidField)
            {
                case "name": application.Name = " "; break;
                case "homepage": application.Homepage = "ftp://example.org"; break;
                case "callbacks": application.CallbackUrls = null!; break;
                case "empty-callbacks": application.CallbackUrls.Clear(); break;
                case "callback": application.CallbackUrls[0] = "relative/callback"; break;
                case "icon": application.IconUrl = "not-a-url"; break;
                case "id": application.Id = " "; break;
                default: throw new ArgumentOutOfRangeException(nameof(invalidField));
            }
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                if (update) { await client.UpdateOAuthAppAsync(application); }
                else { await client.CreateOAuthAppAsync(application); }
            });
        }
    }
}
