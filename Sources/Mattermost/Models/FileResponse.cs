using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    internal class FileResponse
    {
        [JsonPropertyName("file_infos")]
        public IReadOnlyList<FileDetails> Files { get; set; } = Array.Empty<FileDetails>();
    }
}