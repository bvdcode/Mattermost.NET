using Mattermost.Enums;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class UserReportTests
    {
        [Test]
        public async Task Defaults_UseNativeGetWithoutInventedPaging([Values(false, true)] bool count)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/reports/users" + (count ? "/count" : "")));
                Dictionary<string, string> query = ReadQuery(request);
                Assert.That(query["hide_active"], Is.EqualTo("false"));
                Assert.That(query["hide_inactive"], Is.EqualTo("false"));
                Assert.That(query["has_no_team"], Is.EqualTo("false"));
                Assert.That(query.ContainsKey("page"), Is.False);
                Assert.That(query.ContainsKey("per_page"), Is.False);
                Assert.That(query.ContainsKey("guest_filter"), Is.False);
                Assert.That(query.ContainsKey("from_id"), Is.False);
                Assert.That(query.ContainsKey("role_filter"), Is.False);
                if (count)
                {
                    Assert.That(query.Keys, Is.EquivalentTo(new[] { "hide_active", "hide_inactive", "has_no_team" }));
                }
                else
                {
                    Assert.That(query["page_size"], Is.EqualTo("50"));
                    Assert.That(query["sort_column"], Is.EqualTo("Username"));
                    Assert.That(query["sort_direction"], Is.EqualTo("asc"));
                    Assert.That(query["direction"], Is.EqualTo("next"));
                    Assert.That(query["date_range"], Is.EqualTo("all_time"));
                }
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, count ? "0" : "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (count) { Assert.That(await client.GetUserReportCountAsync(), Is.Zero); }
            else { Assert.That(await client.GetUserReportsAsync(), Is.Empty); }
        }

        [Test]
        public async Task SharedFilters_EscapeNativeNamesAndPreserveOverrides([Values(false, true)] bool count)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Dictionary<string, string> query = ReadQuery(request);
                Assert.That(query["team_filter"], Is.EqualTo("team/1"));
                Assert.That(query["role_filter"], Is.EqualTo("role&custom"));
                Assert.That(query["search_term"], Is.EqualTo("имя & + email@example.test"));
                Assert.That(query["guest_filter"], Is.EqualTo("multi_channel"));
                Assert.That(query["has_no_team"], Is.EqualTo("true"));
                Assert.That(query["hide_active"], Is.EqualTo("true"));
                Assert.That(query["hide_inactive"], Is.EqualTo("false"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, count ? "1" : "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (count)
            {
                await client.GetUserReportCountAsync(" team/1 ", " role&custom ", true, true, false,
                    " имя & + email@example.test ", UserReportGuestFilter.MultiChannelGuests);
            }
            else
            {
                await client.GetUserReportsAsync(teamId: " team/1 ", role: " role&custom ", hasNoTeam: true,
                    hideActive: true, searchTerm: " имя & + email@example.test ", guestFilter: UserReportGuestFilter.MultiChannelGuests);
            }
        }

        [TestCase(UserReportSortColumn.Username, "Username")]
        [TestCase(UserReportSortColumn.CreateAt, "CreateAt")]
        [TestCase(UserReportSortColumn.FirstName, "FirstName")]
        [TestCase(UserReportSortColumn.LastName, "LastName")]
        [TestCase(UserReportSortColumn.Nickname, "Nickname")]
        [TestCase(UserReportSortColumn.Email, "Email")]
        [TestCase(UserReportSortColumn.Roles, "Roles")]
        public async Task SortColumns_MapEveryNativeColumn(UserReportSortColumn column, string expected)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(ReadQuery(request)["sort_column"], Is.EqualTo(expected));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUserReportsAsync(sortColumn: column);
        }

        [TestCase(UserReportDateRange.AllTime, "all_time")]
        [TestCase(UserReportDateRange.Last30Days, "last_30_days")]
        [TestCase(UserReportDateRange.PreviousMonth, "previous_month")]
        [TestCase(UserReportDateRange.Last6Months, "last_6_months")]
        public async Task DateRanges_UseActualServerValues(UserReportDateRange range, string expected)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(ReadQuery(request)["date_range"], Is.EqualTo(expected));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUserReportsAsync(dateRange: range);
        }

        [Test]
        public async Task Cursor_PreservesExactColumnValue([Values(false, true)] bool descending, [Values(false, true)] bool previous)
        {
            const string value = "  Имя + & /  ";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Dictionary<string, string> query = ReadQuery(request);
                Assert.That(query["from_id"], Is.EqualTo("user/1"));
                Assert.That(query["from_column_value"], Is.EqualTo(value));
                Assert.That(query["sort_direction"], Is.EqualTo(descending ? "desc" : "asc"));
                Assert.That(query["direction"], Is.EqualTo(previous ? "prev" : "next"));
                Assert.That(query["page_size"], Is.EqualTo("100"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUserReportsAsync(100, UserReportSortColumn.FirstName, descending, " user/1 ", value, previous);
        }

        [TestCase(UserReportGuestFilter.AllGuests, "all")]
        [TestCase(UserReportGuestFilter.SingleChannelGuests, "single_channel")]
        [TestCase(UserReportGuestFilter.MultiChannelGuests, "multi_channel")]
        public async Task GuestFilter_MapsNativeValues(UserReportGuestFilter filter, string expected)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                requests++;
                Assert.That(ReadQuery(request)["guest_filter"], Is.EqualTo(expected));
                bool count = request.RequestUri!.AbsolutePath.EndsWith("/count", StringComparison.Ordinal);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, count ? "0" : "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUserReportsAsync(guestFilter: filter);
            await client.GetUserReportCountAsync(guestFilter: filter);
            Assert.That(requests, Is.EqualTo(2));
        }

        [Test]
        public async Task Report_ParsesProfileAndAllNativeStatistics()
        {
            const string json = "[{\"id\":\"user1\",\"username\":\"report-user\",\"create_at\":4294967297,\"last_login\":4294967298,"
                + "\"last_status_at\":4294967299,\"last_post_date\":4294967300,\"days_active\":4294967301,"
                + "\"total_posts\":4294967302,\"channel_count\":4294967303,\"teams\":\"Alpha, Beta\"}]";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<UserReport> result = await client.GetUserReportsAsync();
            Assert.That(result, Has.Count.EqualTo(1));
            UserReport user = result[0];
            Assert.That(user.Id, Is.EqualTo("user1"));
            Assert.That(user.Username, Is.EqualTo("report-user"));
            Assert.That(user.CreatedAtUnixTimestamp, Is.EqualTo(4294967297L));
            Assert.That(user.LastLoginUnixTimestamp, Is.EqualTo(4294967298L));
            Assert.That(user.LastStatusAtUnixTimestamp, Is.EqualTo(4294967299L));
            Assert.That(user.LastPostDateUnixTimestamp, Is.EqualTo(4294967300L));
            Assert.That(user.DaysActive, Is.EqualTo(4294967301L));
            Assert.That(user.TotalPosts, Is.EqualTo(4294967302L));
            Assert.That(user.ChannelCount, Is.EqualTo(4294967303L));
            Assert.That(user.Teams, Is.EqualTo("Alpha, Beta"));
            JsonElement encoded = JsonSerializer.SerializeToElement(user);
            Assert.That(encoded.GetProperty("total_posts").GetInt64(), Is.EqualTo(4294967302L));
            Assert.That(encoded.GetProperty("last_login").GetInt64(), Is.EqualTo(4294967298L));
        }

        [Test]
        public async Task OptionalStatistics_PreserveUnknownAndZero([Values(false, true)] bool explicitNull)
        {
            string json = explicitNull ? "[{\"channel_count\":0,\"total_posts\":null,\"last_status_at\":null}]" : "[{\"channel_count\":0}]";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            UserReport user = (await client.GetUserReportsAsync())[0];
            Assert.That(user.ChannelCount, Is.Zero);
            Assert.That(user.TotalPosts, Is.Null);
            Assert.That(user.LastStatusAtUnixTimestamp, Is.Null);
            Assert.That(user.LastPostDateUnixTimestamp, Is.Null);
            Assert.That(user.DaysActive, Is.Null);
            Assert.That(user.LastLoginUnixTimestamp, Is.Zero);
            Assert.That(user.Teams, Is.Empty);
        }

        [Test]
        public async Task Count_ReadsScalar64BitInteger([Values(0L, 4294967297L, long.MaxValue)] long value)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, value.ToString(System.Globalization.CultureInfo.InvariantCulture))));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetUserReportCountAsync(), Is.EqualTo(value));
        }

        private static Dictionary<string, string> ReadQuery(HttpRequestMessage request)
        {
            return request.RequestUri!.Query.TrimStart('?').Split('&').Select(part => part.Split('=', 2))
                .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]), StringComparer.Ordinal);
        }
    }
}
