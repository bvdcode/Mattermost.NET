using Mattermost.Models.Users;
using System.Text.Json;

namespace Mattermost.Tests
{
    public partial class MattermostClientTests
    {
        private string username = string.Empty;
        private string password = string.Empty;
        private string token = string.Empty;
        IMattermostClient client;

        [SetUp]
        public void Setup()
        {
            string json = File.ReadAllText("secrets.json");
            var secrets = JsonSerializer.Deserialize<Secrets>(json)!;
            username = secrets.Username;
            password = secrets.Password;
            token = secrets.Token;
            client = new MattermostClient();
        }

        [Test]
        public async Task LoginTest_ValidCredentials_ReturnsToken()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            User result = await client.LoginAsync(username, password);
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Username, Is.Not.Null);
                Assert.That(result.Email, Is.EqualTo(username));
                Assert.That(result.Id, Is.Not.Empty);
                Assert.That(result.CreatedAt, Is.GreaterThan(0));
                Assert.That(result.UpdatedAt, Is.GreaterThan(0));
                Assert.That(result.Username, Is.Not.Empty);
                Assert.That(result.Locale, Is.Not.Empty);
                Assert.That(result.IsBot, Is.False);
                Assert.That(result.Timezone, Is.Not.Null);
                Assert.That(result.AuthData, Is.Not.Null);
            });
        }
    }
}