using System.Text.Json;
using Mattermost.Builders;
using Mattermost.Enums;
using Mattermost.Models;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            string json = File.ReadAllText("secrets.json");
            var secrets = JsonSerializer.Deserialize<Secrets>(json)!;
            using MattermostClient client = new();

            client.OnConnected += (sender, e) =>
            {
                Console.WriteLine($"Connected to {e.Uri}");
                ShowUserInfo(client);
            };

            client.OnStatusUpdated += (sender, e) => Console.WriteLine($"Status updated: {e.UserStatusUpdate.Status} Current user: ({e.IsCurrentUser})");
            client.OnLogMessage += (s, e) => Console.WriteLine(e.Message);
            client.OnDisconnected += (sender, e) => Console.WriteLine($"Disconnected: {e.CloseStatusDescription}");
            client.OnMessageReceived += (sender, e) =>
            {
                Console.WriteLine($"Received message: {e.Message.Post.Text ?? "(empty)"}");
                e.Client.CreatePostAsync(e.Message.Post.ChannelId, "Hello!");
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
            Console.WriteLine($"   Username: {myInfo.Username} ({myInfo.Nickname ?? myInfo.Email})");
            Console.WriteLine($"   Created at: {myInfo.CreatedAt:MM/dd/yyyy}");
            Console.WriteLine($"   Id: {myInfo.Id}");
        }
    }
}