using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        private async Task SetImageAsync(string route, byte[] image, string fileName, CancellationToken cancellationToken)
        {
            if (image is null) { throw new ArgumentNullException(nameof(image)); }
            if (image.Length == 0) { throw new ArgumentException("An image cannot be empty.", nameof(image)); }
            if (string.IsNullOrWhiteSpace(fileName)) { throw new ArgumentException("A filename is required.", nameof(fileName)); }
            cancellationToken.ThrowIfCancellationRequested();
            byte[] data = (byte[])image.Clone();
            string filename = fileName.Trim();
            using MultipartFormDataContent content = new MultipartFormDataContent();
            using ByteArrayContent imageContent = new ByteArrayContent(data);
            content.Add(imageContent, "image", filename);
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Post, route,
                content: content, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw CreateRequestException(response, json, HttpMethod.Post, route);
            }
        }
    }
}
