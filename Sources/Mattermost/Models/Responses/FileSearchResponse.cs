using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Search results in server order, with file metadata keyed by identifier.</summary>
    public class FileSearchResponse
    {
        /// <summary>Matching file identifiers in display order.</summary>
        [JsonPropertyName("order")]
        public IReadOnlyList<string> Order { get; set; } = Array.Empty<string>();

        /// <summary>Metadata keyed by the identifiers in Order.</summary>
        [JsonPropertyName("file_infos")]
        public IReadOnlyDictionary<string, FileDetails> FileInfos { get; set; } = new Dictionary<string, FileDetails>();

        /// <summary>Next-page file identifier when supplied by the search backend; otherwise empty.</summary>
        [JsonPropertyName("next_file_info_id")]
        public string NextFileInfoId { get; set; } = string.Empty;

        /// <summary>Previous-page file identifier when supplied by the search backend; otherwise empty.</summary>
        [JsonPropertyName("prev_file_info_id")]
        public string PreviousFileInfoId { get; set; } = string.Empty;

        /// <summary>Latest inaccessible file's creation time in Unix milliseconds, or zero when none was excluded.</summary>
        [JsonPropertyName("first_inaccessible_file_time")]
        public long FirstInaccessibleFileTime { get; set; }
    }
}
