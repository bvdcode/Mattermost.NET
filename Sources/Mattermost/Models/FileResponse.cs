using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    internal class FileResponse
    {
        [JsonPropertyName("file_infos")]
        public IEnumerable<FileDetails> Files { get; set; } = Enumerable.Empty<FileDetails>();
    }
}