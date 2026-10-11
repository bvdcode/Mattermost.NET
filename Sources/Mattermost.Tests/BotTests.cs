using Mattermost.Models.Bots;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class BotTests
    {
        private const string BotJson = "{\"user_id\":\"bot-user\",\"username\":\"test-bot\",\"display_name\":\"Display\",\"description\":\"Description\",\"owner_id\":\"owner\",\"last_icon_update\":1800000000001,\"create_at\":1800000000002,\"update_at\":1800000000003,\"delete_at\":1800000000004,\"system_owned\":true}";

        [TestCase("list", "/chat/api/v4/bots?page=2&per_page=3&include_deleted=true&only_orphaned=true", "GET")]
        [TestCase("get", "/chat/api/v4/bots/id%2F1?include_deleted=true", "GET")]
        [TestCase("create", "/chat/api/v4/bots", "POST")]
        [TestCase("patch", "/chat/api/v4/bots/id%2F1", "PUT")]
        [TestCase("disable", "/chat/api/v4/bots/id%2F1/disable", "POST")]
        [TestCase("enable", "/chat/api/v4/bots/id%2F1/enable", "POST")]
        [TestCase("assign", "/chat/api/v4/bots/id%2F1/assign/owner%2F2", "POST")]
        public async Task Methods_UseNativeContracts(string operation, string route, string method)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                if (operation == "create" || operation == "patch")
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                    if (operation == "create")
                    {
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                        Assert.That(body.RootElement.GetProperty("username").GetString(), Is.EqualTo("id/1"));
                        Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo(" Display "));
                        Assert.That(body.RootElement.GetProperty("description").GetString(), Is.EqualTo(" Description "));
                    }
                    else
                    {
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                        Assert.That(body.RootElement.GetProperty("description").GetString(), Is.EqualTo(""));
                    }
                }
                else { Assert.That(request.Content, Is.Null); }
                string json = operation == "list" ? "[" + BotJson + "]" : BotJson;
                return ApiTestHttp.Json(HttpStatusCode.OK, json);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object result = await InvokeAsync(client, operation, " id/1 ");
            Bot bot = operation == "list" ? ((IList<Bot>)result).Single() : (Bot)result;
            Assert.Multiple(() =>
            {
                Assert.That(bot.UserId, Is.EqualTo("bot-user"));
                Assert.That(bot.Username, Is.EqualTo("test-bot"));
                Assert.That(bot.DisplayName, Is.EqualTo("Display"));
                Assert.That(bot.Description, Is.EqualTo("Description"));
                Assert.That(bot.OwnerId, Is.EqualTo("owner"));
                Assert.That(bot.LastIconUpdate, Is.EqualTo(1800000000001L));
                Assert.That(bot.CreatedAt, Is.EqualTo(1800000000002L));
                Assert.That(bot.UpdatedAt, Is.EqualTo(1800000000003L));
                Assert.That(bot.DeletedAt, Is.EqualTo(1800000000004L));
                Assert.That(bot.SystemOwned, Is.True);
            });
        }

        [TestCase("username", " changed ", "changed")]
        [TestCase("display_name", "", "")]
        [TestCase("display_name", "Name", "Name")]
        [TestCase("description", "", "")]
        [TestCase("description", "Description", "Description")]
        public async Task Patch_OmitsUnchangedFieldsAndAllowsClearingText(string field, string value, string expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty(field).GetString(), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, BotJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (field)
            {
                case "username": await client.PatchBotAsync("bot", username: value); break;
                case "display_name": await client.PatchBotAsync("bot", displayName: value); break;
                case "description": await client.PatchBotAsync("bot", description: value); break;
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
        }

        [Test]
        public async Task Defaults_UseNormalPagingAndOmitOptionalCreateMetadata()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60&include_deleted=false&only_orphaned=false"));
                    return ApiTestHttp.Json(HttpStatusCode.OK, "[]");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("username").GetString(), Is.EqualTo("bot"));
                return ApiTestHttp.Json(HttpStatusCode.Created, BotJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetBotsAsync();
            await client.CreateBotAsync("bot");
        }

        [TestCase("null")]
        [TestCase("[]")]
        public async Task EmptyBotLists_AreNormalized(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetBotsAsync(), Is.Empty);
        }

        private static async Task<object> InvokeAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return await client.GetBotsAsync(2, 3, true, true, token);
                case "get": return await client.GetBotAsync(id, true, token);
                case "create": return await client.CreateBotAsync(id, " Display ", " Description ", token);
                case "patch": return await client.PatchBotAsync(id, description: "", cancellationToken: token);
                case "disable": return await client.DisableBotAsync(id, token);
                case "enable": return await client.EnableBotAsync(id, token);
                case "assign": return await client.AssignBotAsync(id, " owner/2 ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
