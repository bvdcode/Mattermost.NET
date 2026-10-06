using System;
using System.Net.Http;

namespace Mattermost
{
    internal class ClientInitialization
    {
        public ClientInitialization(Uri serverUri, string? apiKey, HttpClient? httpClient)
        {
            ServerUri = serverUri;
            ApiKey = apiKey;
            HttpClient = httpClient;
        }

        public Uri ServerUri { get; }

        public string? ApiKey { get; }

        public HttpClient? HttpClient { get; }
    }
}
