using Mattermost.Exceptions;
using Mattermost.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        private Task SendRequestAsync(HttpMethod method, string requestUri, object? payload = null, CancellationToken cancellationToken = default) =>
            SendRequestAsync<object>(method, requestUri, payload, cancellationToken);

        private Task SendUnauthenticatedRequestAsync(
            HttpMethod method,
            string requestUri,
            object? payload = null,
            CancellationToken cancellationToken = default) =>
            SendRequestAsync<object>(method, requestUri, payload, cancellationToken, requiresAuthorization: false);

        private async Task<TResult> SendRequestAsync<TResult>(
            HttpMethod method,
            string requestUri,
            object? payload = null,
            CancellationToken cancellationToken = default,
            Func<TResult>? nullResultFactory = null,
            bool requiresAuthorization = true)
        {
            using HttpResponseMessage response = await SendHttpRequestAsync(
                method,
                requestUri,
                payload,
                requiresAuthorization: requiresAuthorization,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw CreateRequestException(response, json, method, requestUri);
            }

            object? result = JsonSerializer.Deserialize<TResult>(json);
            if (result != null)
            {
                return (TResult)result;
            }

            if (nullResultFactory != null)
            {
                return nullResultFactory();
            }

            throw new JsonException("Failed to deserialize result: " + json);
        }

        private MattermostClientException CreateRequestException(HttpResponseMessage response, string json,
            HttpMethod method, string route)
        {
            MattermostClientException exception;
            try
            {
                MattermostApiErrorDetails details = JsonSerializer.Deserialize<MattermostApiErrorDetails>(json)
                    ?? throw new JsonException("Failed to deserialize error result: " + json);
                exception = new MattermostClientException(details.Message);
            }
            catch (Exception)
            {
                exception = new MattermostClientException("Unknown error, server response: " + response.StatusCode);
            }
            exception.StatusCode = response.StatusCode;
            exception.ResponseJson = json;
            exception.RequestUri = BuildRequestUri(route).ToString();
            exception.RequestMethod = method.Method;
            return exception;
        }

        private async Task<byte[]> GetBinaryContentAsync(string route, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Get, route,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw CreateRequestException(response, json, HttpMethod.Get, route);
            }
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendHttpRequestAsync(
            HttpMethod method,
            string route,
            object? payload = null,
            HttpContent? content = null,
            bool requiresAuthorization = true,
            string? authorizationTokenOverride = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (requiresAuthorization)
            {
                await CheckAuthorizedAsync(cancellationToken).ConfigureAwait(false);
            }

            using HttpRequestMessage request = new HttpRequestMessage(method, BuildRequestUri(route));
            bool hasExternalContent = content != null;
            if (hasExternalContent)
            {
                request.Content = content;
            }
            else if (payload != null)
            {
                string jsonPayload = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            string? accessToken = authorizationTokenOverride;
            if (requiresAuthorization && string.IsNullOrWhiteSpace(accessToken))
            {
                accessToken = _accessToken;
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                return await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // External content ownership belongs to the caller.
                if (hasExternalContent)
                {
                    request.Content = null;
                }
            }
        }

        private Uri BuildRequestUri(string route)
        {
            return BuildRequestUri(_serverUri, route);
        }

        private static Uri BuildRequestUri(Uri baseUri, string route)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException("Route cannot be null or empty.", nameof(route));
            }

            bool hasExplicitScheme = route.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal) >= 0;
            if (hasExplicitScheme && Uri.TryCreate(route, UriKind.Absolute, out Uri? absoluteUri))
            {
                return absoluteUri;
            }

            UriBuilder builder = new UriBuilder(baseUri)
            {
                Path = baseUri.AbsolutePath.TrimEnd('/') + "/",
                Query = string.Empty,
                Fragment = string.Empty
            };
            return new Uri(builder.Uri, route.TrimStart('/'));
        }
    }
}
