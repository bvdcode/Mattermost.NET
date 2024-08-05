using Mattermost.Events;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            const string token = "37VlFKySIZn6gryA85cR1GKBQkjmfRZ6";
            const string server = "https://mm.your-server.com";
            MattermostClient client = new(server, token);
            client.OnLogMessage += Client_OnLogMessage;
            client.OnMessageReceived += Client_OnMessageReceived;
            await client.StartReceivingAsync();
        }

        private static void Client_OnMessageReceived(object? sender, MessageEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message.Post.Text))
            {
                return;
            }
            LogEventArgs args = new(e.Message.Post.Text);
            Client_OnLogMessage(sender, args);

            e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello, World!");
        }

        private static void Client_OnLogMessage(object? sender, LogEventArgs e)
        {
            Console.WriteLine("[{0}]: {1}", DateTime.Now, e.Message);
        }
    }
}