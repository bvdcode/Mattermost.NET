using Mattermost.Models;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    [NonParallelizable]
    internal class MattermostClientCustomUrlTokenTests
    {
        [Test]
        public async Task CustomServerAndToken_Autologin_Works()
        {
            const string secretsFileName = "secrets.json";
            if (!File.Exists(secretsFileName))
            {
                Assert.Ignore($"{secretsFileName} not found. Provide custom server URL and token to run this test.");
                return;
            }

            string json = await File.ReadAllTextAsync(secretsFileName).ConfigureAwait(false);
            Secrets? secrets = JsonSerializer.Deserialize<Secrets>(json);
            if (secrets == null)
            {
                Assert.Ignore($"{secretsFileName} is invalid.");
                return;
            }

            if (string.IsNullOrWhiteSpace(secrets.CustomInstance) || string.IsNullOrWhiteSpace(secrets.Token))
            {
                Assert.Ignore($"{secretsFileName} does not contain required fields: customInstance and token.");
                return;
            }

            using MattermostClient client = new(secrets.CustomInstance, secrets.Token);
            var user = await client.GetMeAsync().ConfigureAwait(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Id, Is.Not.Empty);
                Assert.That(client.ServerAddress, Is.EqualTo(new Uri(secrets.CustomInstance)));
                Assert.That(client.CurrentUserInfo.Id, Is.EqualTo(user.Id));
            }
        }
    }
}
