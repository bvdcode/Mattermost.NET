using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Mattermost.Exceptions;
using System.Threading.Tasks;

namespace Mattermost.Extensions
{
    internal static class HttpClientExtensions
    {
        internal static async Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient client, string requestUri, object request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, content);
            return response;
        }

        internal static async Task<TResult> GetResponseAsync<TResult>(this HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AuthorizationException("Unauthorized");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new AuthorizationException("Access denied");
            }
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResult>(responseContent) ?? throw new InvalidOperationException("Response is null");
        }
    }
}