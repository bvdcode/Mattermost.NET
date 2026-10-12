using Mattermost.Models;
using Mattermost.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// File metadata, downloads, image previews and upload sessions.
    /// </summary>
    public interface IMattermostFilesClient
    {
        /// <summary>Search files in one team. Requires Mattermost 5.34 or later, enabled file search and view_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="terms">Nonblank native search expression, including in:channel, from:username or ext:extension filters.</param>
        /// <param name="isOrSearch">Use OR rather than AND for search terms.</param>
        /// <param name="page">Zero-based page; only supported by search backends that implement pagination.</param>
        /// <param name="perPage">Positive page size; only supported by search backends that implement pagination.</param>
        /// <param name="timeZoneOffset">Offset from UTC in seconds for date filters.</param>
        /// <param name="includeDeletedChannels">Include files in archived channels when permitted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Ordered, permission-filtered file metadata. Content matching requires enabled content extraction.</returns>
        Task<FileSearchResponse> SearchTeamFilesAsync(string teamId, string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default);

        /// <summary>Search files across the current user's teams. Requires Mattermost 10.2 or later and enabled file search.</summary>
        /// <param name="terms">Nonblank native search expression. Use SearchTeamFilesAsync to resolve ordinary channel names.</param>
        /// <param name="isOrSearch">Use OR rather than AND for search terms.</param>
        /// <param name="page">Zero-based page; only supported by search backends that implement pagination.</param>
        /// <param name="perPage">Positive page size; only supported by search backends that implement pagination.</param>
        /// <param name="timeZoneOffset">Offset from UTC in seconds for date filters.</param>
        /// <param name="includeDeletedChannels">Include files in archived channels when permitted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Ordered, permission-filtered file metadata. Content matching requires enabled content extraction.</returns>
        Task<FileSearchResponse> SearchFilesAsync(string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default);

        /// <summary>Download a file's preview image.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete preview image as bytes.</returns>
        /// <remarks>Requires channel-read permission or file ownership. Missing previews produce a server error; the original file is not substituted.</remarks>
        Task<byte[]> GetFilePreviewAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Download a file's thumbnail image.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete thumbnail image as bytes.</returns>
        /// <remarks>Requires channel-read permission or file ownership. Missing thumbnails produce a server error.</remarks>
        Task<byte[]> GetFileThumbnailAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Get a file link that can be opened without authentication.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The public file URL.</returns>
        /// <remarks>Requires channel-read permission or file ownership, and public file links enabled on the server. Anyone with the returned link can access the file.</remarks>
        Task<string> GetPublicFileLinkAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Get file HTTP headers without downloading its content. Requires channel-read permission or file ownership.</summary>
        /// <param name="fileId">File identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<FileHeaders> GetFileHeadersAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Get preview image HTTP headers without downloading its content. Missing previews produce a server error.</summary>
        /// <param name="fileId">File identifier. Requires channel-read permission or file ownership.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<FileHeaders> GetFilePreviewHeadersAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Get thumbnail HTTP headers without downloading its content. Missing thumbnails produce a server error.</summary>
        /// <param name="fileId">File identifier. Requires channel-read permission or file ownership.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<FileHeaders> GetFileThumbnailHeadersAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>Create a resumable attachment upload session. Requires Mattermost 5.28 or later, enabled attachments and upload_file permission.</summary>
        /// <param name="channelId">Destination channel identifier.</param>
        /// <param name="fileName">Nonempty file name. The server strips directory components.</param>
        /// <param name="fileSize">Positive total size in bytes, within the server's upload limit.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<UploadSession> CreateUploadAsync(string channelId, string fileName, long fileSize, CancellationToken cancellationToken = default);

        /// <summary>Get an upload session and its current byte offset. Requires the creating account or a system administrator.</summary>
        /// <param name="uploadId">Upload-session identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<UploadSession> GetUploadAsync(string uploadId, CancellationToken cancellationToken = default);

        /// <summary>Append a binary chunk to a resumable upload owned by the current account.</summary>
        /// <param name="uploadId">Upload-session identifier.</param>
        /// <param name="data">Nonempty file bytes starting at the session's current FileOffset. The first chunk must be at least 5 MiB unless it completes the entire file. The buffer is copied before authentication.</param>
        /// <param name="cancellationToken">Cancels authentication and the upload. Read the session offset before resuming after an interrupted request.</param>
        /// <returns>File details when complete, or null for the server's 204 incomplete-upload response. Uploading does not create a post.</returns>
        /// <remarks>Do not send concurrent chunks for one session. The server removes the upload session after completion.</remarks>
        Task<FileDetails?> UploadDataAsync(string uploadId, byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get file by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File bytes. </returns>
        Task<byte[]> GetFileAsync(string fileId);

        /// <summary>
        /// Get file stream by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File stream. </returns>
        Task<Stream> GetFileStreamAsync(string fileId);

        /// <summary>
        /// Get file details by specified identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File details. </returns>
        Task<FileDetails> GetFileDetailsAsync(string fileId);

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="filePath"> File fullname on local device. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        Task<FileDetails> UploadFileAsync(string channelId, string filePath, Action<int>? progressChanged = null);

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="fileName"> Name of the uploaded file. </param>
        /// <param name="stream"> File content. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        Task<FileDetails> UploadFileAsync(string channelId, string fileName, Stream stream, Action<int>? progressChanged = null);

        /// <summary>Get the current user's incomplete upload sessions. Requires Mattermost 5.28 or later.</summary>
        /// <param name="userId">Current user's identifier or me. Other users' sessions cannot be requested, even by administrators.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Completed upload sessions are removed by the server.</remarks>
        Task<IList<UploadSession>> GetUserUploadsAsync(string userId, CancellationToken cancellationToken = default);
    }
}
