﻿using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
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

        internal static TResult GetResponse<TResult>(this HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TResult>(responseContent) ?? throw new InvalidOperationException("Response is null");
        }
    }
}