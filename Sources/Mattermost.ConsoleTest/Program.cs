using System.Text.Json;
using Mattermost.Models;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            string json = File.ReadAllText("secrets.json");
            var secrets = JsonSerializer.Deserialize<Secrets>(json)!;
            MattermostClient client = new();

            client.OnConnected += (sender, e) =>
            {
                Console.WriteLine($"Connected to {e.Uri}");
                ShowUserInfo(client);
            };

            client.OnLogMessage += (s, e) => Console.WriteLine(e.Message);
            client.OnDisconnected += (sender, e) => Console.WriteLine($"Disconnected:  {e.CloseStatusDescription}");
            client.OnMessageReceived += (sender, e) =>
            {
                Console.WriteLine($"Received message: {e.Message.Post.Text ?? "(empty)"}");
                e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello!");
            };

            await client.LoginAsync(secrets.Username, secrets.Password);
            await client.StartReceivingAsync();

            await Task.Delay(10000);

            await client.StopReceivingAsync();
            await client.LogoutAsync();
        }

        private static void ShowUserInfo(MattermostClient client)
        {
            var myInfo = client.CurrentUserInfo;
            Console.WriteLine($"User Infomation:  {myInfo.FirstName} {myInfo.LastName}");
            Console.WriteLine($"   Username: {myInfo.Username} ({myInfo.Nickname})");
            Console.WriteLine($"   Created at: {ConvertUnixTimeMillisecondsToDateTime(myInfo.CreatedAt):MM/dd/yyyy}");
            Console.WriteLine($"   Id: {myInfo.Id}");
        }

        /// <summary>
        /// Converts a Unix timestamp in milliseconds to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="unixTimeMilliseconds">The Unix timestamp in milliseconds.</param>
        /// <returns>A <see cref="DateTime"/> representing the specified timestamp.</returns>
        public static DateTime ConvertUnixTimeMillisecondsToDateTime(long unixTimeMilliseconds)
        {
            // Convert milliseconds to seconds for the base conversion
            long unixTimeSeconds = unixTimeMilliseconds / 1000;

            // Convert the seconds-based timestamp to DateTimeOffset
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds);

            // Add the remaining milliseconds to get the precise DateTime
            return dateTimeOffset.DateTime.AddMilliseconds(unixTimeMilliseconds % 1000);
        }
    }
}