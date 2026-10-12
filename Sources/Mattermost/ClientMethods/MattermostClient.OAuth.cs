using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.OAuth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<OAuthApp>> GetOAuthAppsAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.OAuthApps + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<OAuthApp>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<OAuthApp>());
        }

        /// <inheritdoc />
        public Task<OAuthApp> GetOAuthAppAsync(string appId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.OAuthApps + "/" + EscapeReadIdentifier(appId, nameof(appId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OAuthApp>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<OAuthApp> GetOAuthAppInfoAsync(string appId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.OAuthApps + "/" + EscapeReadIdentifier(appId, nameof(appId)) + "/info";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OAuthApp>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<OAuthApp> CreateOAuthAppAsync(OAuthApp application, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildOAuthAppPayload(application, requireManualUrls: true);
            return SendRequestAsync<OAuthApp>(HttpMethod.Post, Routes.OAuthApps, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OAuthApp> UpdateOAuthAppAsync(OAuthApp application, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (application is null) { throw new ArgumentNullException(nameof(application)); }
            string route = Routes.OAuthApps + "/" + EscapeReadIdentifier(application.Id, nameof(application));
            Dictionary<string, object> body = BuildOAuthAppPayload(application, requireManualUrls: false);
            body.Add("id", application.Id.Trim());
            body.Add("mattermost_app_id", application.MattermostAppId);
            return SendRequestAsync<OAuthApp>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteOAuthAppAsync(string appId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.OAuthApps + "/" + EscapeReadIdentifier(appId, nameof(appId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<OAuthApp> RegenerateOAuthAppSecretAsync(string appId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.OAuthApps + "/" + EscapeReadIdentifier(appId, nameof(appId)) + "/regen_secret";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OAuthApp>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<OAuthApp>> GetAuthorizedOAuthAppsAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/oauth/apps/authorized?"
                + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<OAuthApp>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<OAuthApp>());
        }

        private static Dictionary<string, object> BuildOAuthAppPayload(OAuthApp application, bool requireManualUrls)
        {
            if (application is null) { throw new ArgumentNullException(nameof(application)); }
            if (string.IsNullOrWhiteSpace(application.Name)) { throw new ArgumentException("An application name is required.", nameof(application)); }
            if (application.CallbackUrls is null || application.CallbackUrls.Count == 0)
            {
                throw new ArgumentException("At least one callback URL is required.", nameof(application));
            }
            List<string> callbacks = new List<string>();
            foreach (string callback in application.CallbackUrls)
            {
                if (requireManualUrls) { callbacks.Add(RequireOAuthHttpUrl(callback, nameof(application))); }
                else
                {
                    if (!Uri.TryCreate(callback?.Trim(), UriKind.Absolute, out _))
                    {
                        throw new ArgumentException("OAuth callback URLs must be absolute URLs.", nameof(application));
                    }
                    callbacks.Add(callback!.Trim());
                }
            }
            string homepage = application.Homepage?.Trim() ?? string.Empty;
            if (requireManualUrls || homepage.Length > 0) { homepage = RequireOAuthHttpUrl(homepage, nameof(application)); }
            string iconUrl = string.Empty;
            if (!string.IsNullOrEmpty(application.IconUrl)) { iconUrl = RequireOAuthHttpUrl(application.IconUrl, nameof(application)); }
            return new Dictionary<string, object>
            {
                ["name"] = application.Name.Trim(), ["description"] = application.Description,
                ["icon_url"] = iconUrl, ["callback_urls"] = callbacks,
                ["homepage"] = homepage, ["is_trusted"] = application.IsTrusted
            };
        }

        private static string RequireOAuthHttpUrl(string value, string parameterName)
        {
            if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out Uri? url)
                || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("OAuth application URLs must be absolute HTTP or HTTPS URLs.", parameterName);
            }
            return value!.Trim();
        }
    }
}
