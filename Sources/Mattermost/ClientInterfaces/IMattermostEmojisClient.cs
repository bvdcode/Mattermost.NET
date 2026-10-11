using Mattermost.Models.Emojis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Custom emoji discovery, images and management.
    /// </summary>
    public interface IMattermostEmojisClient
    {
        /// <summary>
        /// Get a page of custom emoji metadata. Requires authentication and enabled custom emojis.
        /// </summary>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Items per page, from 1 to 200.</param>
        /// <param name="sortByName">Sort by emoji name. Requires Mattermost 4.7 or later when enabled.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emojis on the requested page. Built-in emojis are not included.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Page or page size is outside the allowed range.</exception>
        Task<IList<Emoji>> GetEmojisAsync(int page = 0, int perPage = 60, bool sortByName = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata by identifier. Requires authentication and enabled custom emojis.
        /// </summary>
        /// <param name="emojiId">Emoji identifier. Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emoji metadata. A missing emoji produces an API error.</returns>
        /// <exception cref="ArgumentException">The identifier is null, empty, or whitespace.</exception>
        Task<Emoji> GetEmojiAsync(string emojiId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a custom emoji image. Requires authentication and enabled custom emojis.
        /// </summary>
        /// <param name="emojiId">Emoji identifier. Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Image bytes in the format stored on the server. A missing emoji produces an API error.</returns>
        /// <exception cref="ArgumentException">The identifier is null, empty, or whitespace.</exception>
        Task<byte[]> GetEmojiImageAsync(string emojiId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata by name. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="emojiName">Emoji name. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Custom emoji metadata. A missing emoji produces an API error.</returns>
        /// <exception cref="ArgumentException">The name is null or blank after trimming.</exception>
        Task<Emoji> GetEmojiByNameAsync(string emojiName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get custom emoji metadata for multiple names. Requires Mattermost 9.2 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="emojiNames">Nonempty collection containing up to 200 distinct names. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching custom emojis. Missing and built-in names are omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty, contains blank names, or exceeds 200 distinct names.</exception>
        Task<IList<Emoji>> GetEmojisByNamesAsync(IEnumerable<string> emojiNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search custom emoji names. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="term">Nonempty search term. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="prefixOnly">Match only names starting with the term instead of names containing it.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Up to 200 matching custom emojis, sorted by name.</returns>
        /// <exception cref="ArgumentException">The term is null or blank after trimming.</exception>
        Task<IList<Emoji>> SearchEmojisAsync(string term, bool prefixOnly = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find custom emojis whose names start with a prefix. Requires Mattermost 4.7 or later, authentication, and enabled custom emojis.
        /// </summary>
        /// <param name="name">Nonempty name prefix. Surrounding whitespace and colons are removed; case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Up to 100 matching custom emojis, sorted by name.</returns>
        /// <exception cref="ArgumentException">The prefix is null or blank after trimming.</exception>
        Task<IList<Emoji>> AutocompleteEmojisAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>Create a custom emoji owned by the authenticated user.</summary>
        /// <param name="name">Emoji name, optionally surrounded by colons. The server validates and lowercases names.</param>
        /// <param name="image">Nonempty image bytes. The server validates format, dimensions and upload size.</param>
        /// <param name="fileName">Image filename with its extension, such as build.png.</param>
        /// <param name="cancellationToken">Cancels authentication and the upload.</param>
        /// <remarks>Requires EnableCustomEmoji and create_emojis permission. Sends multipart form data with the actual authenticated creator ID. Existing emoji names cannot be overwritten.</remarks>
        Task<Emoji> CreateEmojiAsync(string name, byte[] image, string fileName, CancellationToken cancellationToken = default);

        /// <summary>Delete a custom emoji. Requires delete_emojis permission; deleting another user's emoji also requires delete_others_emojis.</summary>
        /// <param name="emojiId">Custom emoji identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteEmojiAsync(string emojiId, CancellationToken cancellationToken = default);
    }
}
