namespace Mattermost.Tests
{
    internal class TypingTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SendTyping_InvalidChannel_RejectsBeforeConnection(string? channelId)
        {
            using var client = new MattermostClient("http://127.0.0.1:1", "test-token");
            Assert.ThrowsAsync<ArgumentException>(() => client.SendTypingAsync(channelId!));
        }

        [Test]
        public void SendTyping_NullParent_RejectsBeforeConnection()
        {
            using var client = new MattermostClient("http://127.0.0.1:1", "test-token");
            Assert.ThrowsAsync<ArgumentNullException>(() => client.SendTypingAsync("channel", null!));
        }

        [Test]
        public void SendTyping_Disconnected_DoesNotStartAConnection()
        {
            using var client = new MattermostClient("http://127.0.0.1:1", "test-token");
            Assert.ThrowsAsync<InvalidOperationException>(() => client.SendTypingAsync("channel"));
        }

        [Test]
        public void SendTyping_Canceled_DoesNotSend()
        {
            using var client = new MattermostClient("http://127.0.0.1:1", "test-token");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.ThrowsAsync<TaskCanceledException>(() => client.SendTypingAsync("channel", cancellationToken: cancellation.Token));
        }

        [Test]
        public void SendTyping_Disposed_Rejects()
        {
            var client = new MattermostClient("http://127.0.0.1:1", "test-token");
            client.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(() => client.SendTypingAsync("channel"));
        }
    }
}
