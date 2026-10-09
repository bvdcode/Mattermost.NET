using Mattermost.Constants;
using Mattermost.Models.Users;
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
        public Task<IList<Preference>> GetPreferencesAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = GetPreferencesRoute(userId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Preference>>(HttpMethod.Get, url, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<Preference>());
        }

        /// <inheritdoc />
        public Task<IList<Preference>> GetPreferencesByCategoryAsync(string userId, string category,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = GetPreferencesRoute(userId) + "/" + EscapePreferenceKey(category, nameof(category));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Preference>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Preference> GetPreferenceAsync(string userId, string category, string name,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = GetPreferencesRoute(userId) + "/" + EscapePreferenceKey(category, nameof(category))
                + "/name/" + EscapePreferenceKey(name, nameof(name));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Preference>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdatePreferencesAsync(string userId, IEnumerable<Preference> preferences,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = GetPreferencesRoute(userId);
            cancellationToken.ThrowIfCancellationRequested();
            List<Preference> body = PreparePreferenceBatch(preferences);
            return SendRequestAsync(HttpMethod.Put, url, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeletePreferencesAsync(string userId, IEnumerable<Preference> preferences,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = GetPreferencesRoute(userId) + "/delete";
            cancellationToken.ThrowIfCancellationRequested();
            List<Preference> body = PreparePreferenceBatch(preferences);
            return SendRequestAsync(HttpMethod.Post, url, body, cancellationToken);
        }

        private static string GetPreferencesRoute(string userId) => Routes.Users + "/"
            + EscapeReadIdentifier(userId, nameof(userId)) + "/preferences";

        private static string EscapePreferenceKey(string key, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Preference key cannot be blank.", parameterName);
            }
            return Uri.EscapeDataString(key);
        }

        private static List<Preference> PreparePreferenceBatch(IEnumerable<Preference> preferences)
        {
            if (preferences is null)
            {
                throw new ArgumentNullException(nameof(preferences));
            }
            List<Preference> result = new List<Preference>();
            foreach (Preference preference in preferences)
            {
                if (preference is null || string.IsNullOrWhiteSpace(preference.UserId)
                    || string.IsNullOrWhiteSpace(preference.Category) || preference.Name is null || preference.Value is null)
                {
                    throw new ArgumentException("Each preference requires a user ID, category, non-null name and non-null value.", nameof(preferences));
                }
                result.Add(new Preference
                {
                    UserId = preference.UserId, Category = preference.Category, Name = preference.Name, Value = preference.Value
                });
                if (result.Count > MattermostApiLimits.MaxPreferencesPerRequest)
                {
                    throw new ArgumentException("At most 100 preferences can be sent in one request.", nameof(preferences));
                }
            }
            if (result.Count == 0)
            {
                throw new ArgumentException("At least one preference is required.", nameof(preferences));
            }
            return result;
        }
    }
}
