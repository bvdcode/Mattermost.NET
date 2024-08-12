using Mattermost.Events;

namespace Mattermost.ConsoleTest
{
    public class Program
    {
        public static async Task Main()
        {
            string token = "your token";
            MattermostClient client = new();

            client.OnConnected += async (sender, e) =>
            {
                Console.WriteLine($"Connected to {e.Uri}");
                await ShowUserInfoAsync(client);
            };

            client.OnDisconnected += (sender, e) => Console.WriteLine($"Disconnected:  {e.CloseStatusDescription}");

            client.OnMessageReceived += (sender, e) =>
            {
                Console.WriteLine($"Received message: {e.Message.Post.Text ?? "(empty)"}");
                e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello!");
            };

            client.OnLogMessage += (s, e) => Console.WriteLine($"[LOG]: {e.Message}");

            await client.LoginAsync(token);
            await client.StartReceivingAsync();

            await Task.Delay(10000);

            await client.StopReceivingAsync();
            await client.LogoutAsync();
        }

        private static async Task ShowUserInfoAsync(MattermostClient client)
        {
            var myInfo = await client.GetMeAsync();
            Console.WriteLine($"User Infomation:  {myInfo.FirstName} {myInfo.LastName}");
            Console.WriteLine($"   {myInfo.Username} ({myInfo.Nickname})");
            Console.WriteLine($"   {ConvertUnixTimeMillisecondsToDateTime(myInfo.CreatedAt).ToString("MM/dd/yyyy")}");
            Console.WriteLine($"   {myInfo.Id}");
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