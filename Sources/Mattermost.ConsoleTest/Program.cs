using Mattermost.Events;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            string token = "your token";
            MattermostClient client = new();

            client.OnMessageReceived += OnMessageReceived;
            client.OnLogMessage += OnLogMessage;

            await client.LoginAsync(token);
            await client.StartReceivingAsync();

            await Task.Delay(10000);

            await client.StopReceivingAsync();
            await client.LogoutAsync();
        }

        private static void OnLogMessage(object? sender, LogEventArgs e)
        {
            Console.WriteLine($"[LOG]: {e.Message}");
        }

        private static void OnMessageReceived(object? sender, MessageEventArgs e)
        {
            Console.WriteLine($"Received message: {e.Message.Post.Text ?? "empty"}");
            e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello!");
        }
    }
}