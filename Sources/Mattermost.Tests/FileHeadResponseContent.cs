using System.Net;

namespace Mattermost.Tests
{
    internal class FileHeadResponseContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            throw new AssertionException("A HEAD response must not be downloaded.");
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 321;
            return true;
        }
    }
}
