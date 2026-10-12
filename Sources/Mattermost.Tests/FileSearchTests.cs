using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class FileSearchTests
    {
        private const string Results = "{\"order\":[\"file-2\",\"file-1\"],\"file_infos\":{\"file-1\":{\"id\":\"file-1\",\"name\":\"notes.txt\",\"size\":9007199254740993},\"file-2\":{\"id\":\"file-2\",\"user_id\":\"uploader\",\"channel_id\":\"channel\",\"name\":\"agenda.pdf\",\"mime_type\":\"application/pdf\",\"create_at\":5000000000}},\"next_file_info_id\":\"next\",\"prev_file_info_id\":\"previous\",\"first_inaccessible_file_time\":6000000000}";
        private const string EmptyResults = "{\"order\":[],\"file_infos\":{},\"next_file_info_id\":\"\",\"prev_file_info_id\":\"\",\"first_inaccessible_file_time\":0}";

        [TestCase(false, "/chat/api/v4/files/search")]
        [TestCase(true, "/chat/api/v4/teams/team%2F1/files/search")]
        public async Task Search_UsesNativeJsonPostAndAllOptions(bool team, string path)
        {
            const string terms = "  \"meeting notes\"  in:docs ext:pdf +&?#  ";
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(path));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                using JsonDocument payload = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
                JsonElement body = payload.RootElement;
                Assert.That(body.EnumerateObject().Count(), Is.EqualTo(6));
                Assert.That(body.GetProperty("terms").GetString(), Is.EqualTo(terms.Trim()));
                Assert.That(body.GetProperty("is_or_search").GetBoolean(), Is.True);
                Assert.That(body.GetProperty("page").GetInt32(), Is.EqualTo(2));
                Assert.That(body.GetProperty("per_page").GetInt32(), Is.EqualTo(3));
                Assert.That(body.GetProperty("time_zone_offset").GetInt32(), Is.EqualTo(-28800));
                Assert.That(body.GetProperty("include_deleted_channels").GetBoolean(), Is.True);
                return ApiTestHttp.Json(HttpStatusCode.OK, Results);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            FileSearchResponse response;
            if (team) { response = await client.SearchTeamFilesAsync(" team/1 ", terms, true, 2, 3, -28800, true); }
            else { response = await client.SearchFilesAsync(terms, true, 2, 3, -28800, true); }
            Assert.That(response.Order, Is.EqualTo(new[] { "file-2", "file-1" }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Search_SendsNativeDefaults(bool team)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(payload.RootElement.GetProperty("is_or_search").GetBoolean(), Is.False);
                Assert.That(payload.RootElement.GetProperty("page").GetInt32(), Is.Zero);
                Assert.That(payload.RootElement.GetProperty("per_page").GetInt32(), Is.EqualTo(60));
                Assert.That(payload.RootElement.GetProperty("time_zone_offset").GetInt32(), Is.Zero);
                Assert.That(payload.RootElement.GetProperty("include_deleted_channels").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, EmptyResults);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, team);
        }

        [Test]
        public async Task Results_PreserveNativeOrderMetadataCursorsAndLongValues()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Results)));
            using MattermostClient client = ApiTestHttp.Client(http);
            FileSearchResponse result = await client.SearchFilesAsync("notes");
            Assert.That(result.Order, Is.EqualTo(new[] { "file-2", "file-1" }));
            Assert.That(result.FileInfos.Keys, Is.EquivalentTo(result.Order));
            Assert.That(result.FileInfos["file-1"].Size, Is.EqualTo(9007199254740993));
            Assert.That(result.FileInfos["file-2"].Name, Is.EqualTo("agenda.pdf"));
            Assert.That(result.FileInfos["file-2"].UserId, Is.EqualTo("uploader"));
            Assert.That(result.FileInfos["file-2"].ChannelId, Is.EqualTo("channel"));
            Assert.That(result.FileInfos["file-2"].MimeType, Is.EqualTo("application/pdf"));
            Assert.That(result.FileInfos["file-2"].CreateAt, Is.EqualTo(5000000000));
            Assert.That(result.NextFileInfoId, Is.EqualTo("next"));
            Assert.That(result.PreviousFileInfoId, Is.EqualTo("previous"));
            Assert.That(result.FirstInaccessibleFileTime, Is.EqualTo(6000000000));
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(result));
            Assert.That(serialized.RootElement.EnumerateObject().Count(), Is.EqualTo(5));
            Assert.That(serialized.RootElement.GetProperty("file_infos").GetProperty("file-1").GetProperty("size").GetInt64(),
                Is.EqualTo(9007199254740993));
            Assert.That(serialized.RootElement.GetProperty("prev_file_info_id").GetString(), Is.EqualTo("previous"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Search_PreservesNativeEmptyResults(bool team)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, EmptyResults)));
            using MattermostClient client = ApiTestHttp.Client(http);
            FileSearchResponse result = await InvokeAsync(client, team);
            Assert.That(result.Order, Is.Empty);
            Assert.That(result.FileInfos, Is.Empty);
            Assert.That(result.NextFileInfoId, Is.Empty);
            Assert.That(result.PreviousFileInfoId, Is.Empty);
            Assert.That(result.FirstInaccessibleFileTime, Is.Zero);
        }

        [TestCase(false, "null")]
        [TestCase(false, "[]")]
        [TestCase(false, "{invalid")]
        [TestCase(false, "{\"order\":\"file\"}")]
        [TestCase(false, "{\"file_infos\":[]}")]
        [TestCase(true, "null")]
        [TestCase(true, "[]")]
        [TestCase(true, "{invalid")]
        [TestCase(true, "{\"order\":\"file\"}")]
        [TestCase(true, "{\"file_infos\":[]}")]
        public async Task Search_RejectsMalformedObjectResponses(bool team, string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(async () => await InvokeAsync(client, team));
        }

        [TestCase(false, HttpStatusCode.BadRequest)]
        [TestCase(false, HttpStatusCode.Forbidden)]
        [TestCase(false, HttpStatusCode.NotImplemented)]
        [TestCase(true, HttpStatusCode.BadRequest)]
        [TestCase(true, HttpStatusCode.Forbidden)]
        [TestCase(true, HttpStatusCode.NotImplemented)]
        public async Task Search_PreservesNativeErrors(bool team, HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status,
                "{\"id\":\"file_search_unavailable\",\"message\":\"Unavailable\",\"status_code\":" + (int)status + "}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(async () => await InvokeAsync(client, team));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
        }

        [TestCase(false, null)]
        [TestCase(false, "")]
        [TestCase(false, " ")]
        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(true, " ")]
        public async Task Search_RejectsBlankTermsBeforeRequests(bool team, string? terms)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                if (team) { await client.SearchTeamFilesAsync("team", terms!); }
                else { await client.SearchFilesAsync(terms!); }
            });
            Assert.That(error!.ParamName, Is.EqualTo("terms"));
        }

        [TestCase(false, -1, 1, "page")]
        [TestCase(false, 0, 0, "perPage")]
        [TestCase(false, 0, -1, "perPage")]
        [TestCase(true, -1, 1, "page")]
        [TestCase(true, 0, 0, "perPage")]
        [TestCase(true, 0, -1, "perPage")]
        public async Task Search_RejectsInvalidPaginationBeforeRequests(bool team, int page, int perPage, string parameter)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentOutOfRangeException? error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                if (team) { await client.SearchTeamFilesAsync("team", "notes", page: page, perPage: perPage); }
                else { await client.SearchFilesAsync("notes", page: page, perPage: perPage); }
            });
            Assert.That(error!.ParamName, Is.EqualTo(parameter));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task TeamSearch_RejectsInvalidTeamBeforeRequests(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(async () => await client.SearchTeamFilesAsync(id!, "notes"));
            Assert.That(error!.ParamName, Is.EqualTo("teamId"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Search_RejectsDisposedClient(bool team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await InvokeAsync(client, team));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Search_CancelsBeforeAuthenticationOrRequests(bool team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await InvokeAsync(client, team, cancellation.Token));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Search_CancelsInFlightRequest(bool team)
        {
            TaskCompletionSource<bool> entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                entered.SetResult(true);
                await Task.Delay(Timeout.Infinite, token);
                throw new AssertionException("Expected cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            Task request = InvokeAsync(client, team, cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await request);
        }

        private static Task<FileSearchResponse> InvokeAsync(IMattermostFilesClient client, bool team, CancellationToken token = default)
        {
            if (team) { return client.SearchTeamFilesAsync("team", "notes", cancellationToken: token); }
            return client.SearchFilesAsync("notes", cancellationToken: token);
        }
    }
}
