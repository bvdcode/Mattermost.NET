using Mattermost.Events;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            string token = "your token";
            MattermostClient client = new(token);
            client.OnMessageReceived += OnMessageReceived;
            client.OnLogMessage += OnLogMessage;
            await client.StartReceivingAsync();
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