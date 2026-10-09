using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Emojis;
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
        public Task<IList<Emoji>> GetEmojisAsync(int page = 0, int perPage = 60, bool sortByName = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (perPage > MattermostApiLimits.MaxEmojisPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), "Items per page cannot exceed 200.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (sortByName)
            {
                query += "&sort=name";
            }
            return SendRequestAsync<IList<Emoji>>(HttpMethod.Get, Routes.Emojis + "?" + query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Emoji> GetEmojiAsync(string emojiId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedId = EscapeReadIdentifier(emojiId, nameof(emojiId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Emoji>(HttpMethod.Get, Routes.Emojis + "/" + escapedId,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Emoji> GetEmojiByNameAsync(string emojiName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string name = SanitizeEmojiName(emojiName, nameof(emojiName));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Emoji>(HttpMethod.Get, Routes.Emojis + "/name/" + Uri.EscapeDataString(name),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Emoji>> GetEmojisByNamesAsync(IEnumerable<string> emojiNames,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (emojiNames is null)
            {
                throw new ArgumentNullException(nameof(emojiNames));
            }
            cancellationToken.ThrowIfCancellationRequested();
            List<string> names = new List<string>();
            HashSet<string> uniqueNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (string emojiName in emojiNames)
            {
                string name = SanitizeEmojiName(emojiName, nameof(emojiNames));
                if (uniqueNames.Add(name))
                {
                    names.Add(name);
                    if (names.Count > MattermostApiLimits.MaxEmojiNamesPerRequest)
                    {
                        throw new ArgumentException("A bulk lookup cannot contain more than 200 distinct emoji names.", nameof(emojiNames));
                    }
                }
            }
            if (names.Count == 0)
            {
                throw new ArgumentException("At least one emoji name is required.", nameof(emojiNames));
            }
            return SendRequestAsync<IList<Emoji>>(HttpMethod.Post, Routes.Emojis + "/names", names, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Emoji>> SearchEmojisAsync(string term, bool prefixOnly = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string name = SanitizeEmojiName(term, nameof(term));
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { term = name, prefix_only = prefixOnly };
            return SendRequestAsync<IList<Emoji>>(HttpMethod.Post, Routes.Emojis + "/search", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Emoji>> AutocompleteEmojisAsync(string name, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string prefix = SanitizeEmojiName(name, nameof(name));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Emoji>>(HttpMethod.Get, Routes.Emojis + "/autocomplete?name=" + Uri.EscapeDataString(prefix),
                cancellationToken: cancellationToken);
        }
    }
}
