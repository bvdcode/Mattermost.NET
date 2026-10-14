using Mattermost.Exceptions;
using Mattermost.Models;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PerformanceReportApiTests
    {
        [TestCase("")]
        [TestCase("{\"status\":\"OK\"}")]
        public async Task Request_PreservesNativeMapsAndFractionalValues(string response)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/client_perf"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("version").GetString(), Is.EqualTo("0.1.0"));
                Assert.That(body.RootElement.GetProperty("client_id").GetString(), Is.EqualTo("test-client"));
                Assert.That(body.RootElement.GetProperty("start").GetDouble(), Is.EqualTo(1000.25));
                Assert.That(body.RootElement.GetProperty("end").GetDouble(), Is.EqualTo(1001.75));
                Assert.That(body.RootElement.GetProperty("labels").GetProperty("platform").GetString(), Is.EqualTo("windows"));
                Assert.That(body.RootElement.GetProperty("labels").GetProperty("agent").GetString(), Is.EqualTo("other"));
                JsonElement counter = body.RootElement.GetProperty("counters")[0];
                Assert.That(counter.GetProperty("metric").GetString(), Is.EqualTo("long_tasks"));
                Assert.That(counter.GetProperty("value").GetDouble(), Is.EqualTo(2.5));
                Assert.That(counter.TryGetProperty("labels", out _), Is.False);
                JsonElement histogram = body.RootElement.GetProperty("histograms")[0];
                Assert.That(histogram.GetProperty("metric").GetString(), Is.EqualTo("plugin_webapp_perf"));
                Assert.That(histogram.GetProperty("value").GetDouble(), Is.EqualTo(1.25));
                Assert.That(histogram.GetProperty("labels").GetProperty("plugin_id").GetString(), Is.EqualTo("example"));
                Assert.That(histogram.GetProperty("labels").GetProperty("plugin_metric_label").GetString(), Is.EqualTo("load"));
                Assert.That(histogram.TryGetProperty("timestamp", out _), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, response);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SubmitPerformanceReportAsync(Report());
        }

        [Test]
        public async Task Report_IsCopiedBeforeAuthentication()
        {
            PerformanceReport report = Report();
            string expected = JsonSerializer.Serialize(report);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    report.Version = "changed";
                    report.ClientId = "changed";
                    report.Start = 0;
                    report.End = 0;
                    report.Labels!["platform"] = "changed";
                    report.Counters![0].Metric = "changed";
                    report.Counters[0].Value = 0;
                    report.Counters.Clear();
                    report.Histograms![0].Labels!.Clear();
                    report.Histograms.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SubmitPerformanceReportAsync(report);
        }

        [TestCase(null)]
        [TestCase("empty")]
        public async Task OptionalListsAndLabels_UseNativeShape(string? kind)
        {
            PerformanceReport report = new PerformanceReport { Start = 1, End = 2 };
            if (kind == "empty")
            {
                report.Counters = new List<MetricSample>();
                report.Histograms = new List<MetricSample>();
                report.Labels = new Dictionary<string, string>();
            }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(json.RootElement.GetProperty("labels").ValueKind, Is.EqualTo(kind is null ? JsonValueKind.Null : JsonValueKind.Object));
                Assert.That(json.RootElement.GetProperty("counters").ValueKind, Is.EqualTo(kind is null ? JsonValueKind.Null : JsonValueKind.Array));
                Assert.That(json.RootElement.GetProperty("histograms").ValueKind, Is.EqualTo(kind is null ? JsonValueKind.Null : JsonValueKind.Array));
                return ApiTestHttp.Json(HttpStatusCode.OK, "");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SubmitPerformanceReportAsync(report);
        }

        [TestCase(double.NaN, 2)]
        [TestCase(1, double.NaN)]
        [TestCase(double.NegativeInfinity, 2)]
        [TestCase(1, double.PositiveInfinity)]
        [TestCase(3, 2)]
        public async Task InvalidTimestamps_RejectBeforeHttp(double start, double end)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SubmitPerformanceReportAsync(new PerformanceReport { Start = start, End = end }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task InvalidVersion_RejectsBeforeHttp(string? version)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SubmitPerformanceReportAsync(new PerformanceReport { Version = version! }));
        }

        [TestCase("counter-null")]
        [TestCase("histogram-null")]
        [TestCase("metric-empty")]
        [TestCase("metric-null")]
        [TestCase("nan")]
        [TestCase("infinity")]
        public async Task InvalidSamples_RejectBeforeHttp(string kind)
        {
            PerformanceReport report = Report();
            switch (kind)
            {
                case "counter-null": report.Counters![0] = null!; break;
                case "histogram-null": report.Histograms![0] = null!; break;
                case "metric-empty": report.Counters![0].Metric = " "; break;
                case "metric-null": report.Counters![0].Metric = null!; break;
                case "nan": report.Counters![0].Value = double.NaN; break;
                case "infinity": report.Histograms![0].Value = double.PositiveInfinity; break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SubmitPerformanceReportAsync(report));
        }

        [TestCase(HttpStatusCode.BadRequest, "{\"message\":\"invalid report\"}")]
        [TestCase(HttpStatusCode.Forbidden, "{\"message\":\"denied\"}")]
        [TestCase(HttpStatusCode.InternalServerError, "not JSON")]
        public async Task NativeErrors_PreserveStatusAndResponse(HttpStatusCode status, string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.SubmitPerformanceReportAsync(Report()));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
            Assert.That(error.RequestUri, Is.EqualTo("https://mattermost.example/chat/api/v4/client_perf"));
        }

        [Test]
        public async Task CancellationDisposalAndNullReport_RejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SubmitPerformanceReportAsync(null!));
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => client.SubmitPerformanceReportAsync(Report(), source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.SubmitPerformanceReportAsync(Report()));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task InFlightCancellation_ReachesAuthenticationAndRequest(bool authentication)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = client.SubmitPerformanceReportAsync(Report(), source.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static PerformanceReport Report() => new PerformanceReport
        {
            ClientId = "test-client", Start = 1000.25, End = 1001.75,
            Labels = new Dictionary<string, string> { ["platform"] = "windows", ["agent"] = "other" },
            Counters = new List<MetricSample> { new MetricSample { Metric = "long_tasks", Value = 2.5 } },
            Histograms = new List<MetricSample> { new MetricSample
            {
                Metric = "plugin_webapp_perf", Value = 1.25,
                Labels = new Dictionary<string, string> { ["plugin_id"] = "example", ["plugin_metric_label"] = "load" }
            } }
        };
    }
}
