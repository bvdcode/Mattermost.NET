namespace Mattermost.Tests
{
    [TestFixture]
    internal class FileTransferValidationTests
    {
        [Test]
        public async Task BlankIdentifiers_RejectBeforeHttp([Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetFileHeadersAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetFilePreviewHeadersAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetFileThumbnailHeadersAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUploadAsync(value!, "file.txt", 1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUploadAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UploadDataAsync(value!, new byte[] { 1 }));
        }

        [Test]
        public async Task InvalidUploadInputs_RejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUploadAsync("channel", null!, 1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUploadAsync("channel", " ", 1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.CreateUploadAsync("channel", "file.txt", 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.CreateUploadAsync("channel", "file.txt", -1));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UploadDataAsync("upload", null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UploadDataAsync("upload", Array.Empty<byte>()));
        }
    }
}
