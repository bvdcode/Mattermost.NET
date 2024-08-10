using System.Text.Json.Serialization;

namespace Mattermost.Tests
{
    public partial class MattermostClientTests
    {
        public class Secrets
        {
            [JsonPropertyName("username")]
            public string Username { get; set; } = string.Empty;

            [JsonPropertyName("password")]
            public string Password { get; set; } = string.Empty;

            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;
        }
    }
}