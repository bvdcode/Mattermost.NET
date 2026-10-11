using Mattermost.Exceptions;
using System.Net;
using System.Net.Http.Headers;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ImageManagementTests
    {
        [TestCase("get-team", "GET", "/chat/api/v4/teams/id%2F1/image")]
        [TestCase("set-team", "POST", "/chat/api/v4/teams/id%2F1/image")]
        [TestCase("delete-team", "DELETE", "/chat/api/v4/teams/id%2F1/image")]
        [TestCase("set-user", "POST", "/chat/api/v4/users/id%2F1/image")]
        [TestCase("delete-user", "DELETE", "/chat/api/v4/users/id%2F1/image")]
        public async Task Requests_UseNativeRoutes(string operation, string method, string route)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                if (method == "POST")
                {
                    MultipartFormDataContent content = (MultipartFormDataContent)request.Content!;
                    Assert.That(content.Count(), Is.EqualTo(1));
                    HttpContent image = content.Single();
                    ContentDispositionHeaderValue disposition = image.Headers.ContentDisposition!;
                    Assert.That(disposition.Name?.Trim('"'), Is.EqualTo("image"));
                    Assert.That(disposition.FileName?.Trim('"'), Is.EqualTo("image.png"));
                    Assert.That(await image.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                }
                else { Assert.That(request.Content, Is.Null); }
                return method == "GET"
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[] { 4, 5, 6 }) }
                    : ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (operation == "get-team")
            {
                Assert.That(await client.GetTeamImageAsync(" id/1 "), Is.EqualTo(new byte[] { 4, 5, 6 }));
            }
            else { await InvokeAsync(client, operation, " id/1 "); }
        }

        [Test]
        public async Task BlankIdentifiers_AreRejectedBeforeHttp(
            [Values("get-team", "set-team", "delete-team", "set-user", "delete-user")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
        }

        [Test]
        public async Task InvalidUploadData_IsRejectedBeforeHttp([Values(false, true)] bool team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => SetAsync(client, team, null!, "image.png"));
            await Assert.ThrowsAsync<ArgumentException>(() => SetAsync(client, team, Array.Empty<byte>(), "image.png"));
            foreach (string? filename in new string?[] { null, "", " " })
            {
                await Assert.ThrowsAsync<ArgumentException>(() => SetAsync(client, team, new byte[] { 1 }, filename!));
            }
        }

        [Test]
        public async Task Upload_SnapshotsBytesBeforeAuthentication([Values(false, true)] bool team)
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                Assert.That(await ((MultipartFormDataContent)request.Content!).Single().ReadAsByteArrayAsync(token),
                    Is.EqualTo(new byte[] { 1, 2, 3 }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            byte[] image = new byte[] { 1, 2, 3 };
            Task pending = SetAsync(client, team, image, "image.png");
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            image[0] = 9;
            release.SetResult();
            await pending.WaitAsync(TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task Upload_DisposesMultipartContent([Values(false, true)] bool team)
        {
            HttpContent? upload = null;
            HttpContent? responseContent = null;
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                upload = request.Content;
                HttpResponseMessage response = ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
                responseContent = response.Content;
                return Task.FromResult(response);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await SetAsync(client, team, new byte[] { 1 }, "image.png");
            await Assert.ThrowsAsync<ObjectDisposedException>(() => upload!.ReadAsByteArrayAsync());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => responseContent!.ReadAsByteArrayAsync());
        }

        [Test]
        public async Task Errors_PropagateNativeDetails(
            [Values("get-team", "set-team", "delete-team", "set-user", "delete-user")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "id"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("get-team", "set-team", "delete-team", "set-user", "delete-user")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, "id", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, "id"));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp(
            [Values("get-team", "set-team", "delete-team", "set-user", "delete-user")] string operation)
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
            Task pending = InvokeAsync(client, operation, "id", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task SetAsync(MattermostClient client, bool team, byte[] image, string filename)
        {
            if (team) { return client.SetTeamImageAsync("id", image, filename); }
            return client.SetUserImageAsync("id", image, filename);
        }

        private static Task InvokeAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "get-team": return client.GetTeamImageAsync(id, token);
                case "set-team": return client.SetTeamImageAsync(id, new byte[] { 1, 2, 3 }, " image.png ", token);
                case "delete-team": return client.DeleteTeamImageAsync(id, token);
                case "set-user": return client.SetUserImageAsync(id, new byte[] { 1, 2, 3 }, " image.png ", token);
                case "delete-user": return client.DeleteUserImageAsync(id, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
