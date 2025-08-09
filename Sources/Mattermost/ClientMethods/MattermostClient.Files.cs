using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Mattermost.Models;
using Mattermost.Constants;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get file by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File bytes. </returns>
        public async Task<byte[]> GetFileAsync(string fileId)
        {
            CheckDisposed();
            await CheckAuthorizedAsync();
            string url = Routes.Files + "/" + fileId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Get file stream by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File stream. </returns>
        public async Task<Stream> GetFileStreamAsync(string fileId)
        {
            CheckDisposed();
            await CheckAuthorizedAsync();
            string url = Routes.Files + "/" + fileId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Get file details by specified identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File details. </returns>
        public Task<FileDetails> GetFileDetailsAsync(string fileId)
        {
            CheckDisposed();
            return SendRequestAsync<FileDetails>(HttpMethod.Get, Routes.Files + "/" + fileId + "/info");
        }

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="filePath"> File fullname on local device. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        public async Task<FileDetails> UploadFileAsync(string channelId, string filePath, Action<int>? progressChanged = null)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            using var fs = fileInfo.OpenRead();
            return await UploadFileAsync(channelId, fileInfo.Name, fs, progressChanged);
        }

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="fileName"> Name of the uploaded file. </param>
        /// <param name="stream"> File content. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        public async Task<FileDetails> UploadFileAsync(string channelId, string fileName, Stream stream, Action<int>? progressChanged = null)
        {
            CheckDisposed();
            await CheckAuthorizedAsync();
            string url = $"{Routes.Files}?channel_id={channelId}";
            MultipartFormDataContent content = new MultipartFormDataContent();
            StreamContent file = new StreamContent(stream);
            content.Add(file, "files", fileName);
            CancellationTokenSource cts = new CancellationTokenSource();
            if (progressChanged != null)
            {
                StartProgressTracker(stream, cts.Token, progressChanged);
            }
            var result = await _http.PostAsync(url, content);
            result = result.EnsureSuccessStatusCode();
            cts.Cancel();
            string json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<FileResponse>(json)
                ?? throw new JsonException("Failed to deserialize file response: " + json);
            return response.Files.Single();
        }
    }
}
