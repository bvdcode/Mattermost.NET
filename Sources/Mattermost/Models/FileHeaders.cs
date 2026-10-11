using System;

namespace Mattermost.Models
{
    /// <summary>HTTP metadata returned by a file, preview or thumbnail HEAD request.</summary>
    public class FileHeaders
    {
        /// <summary>Content length in bytes, when provided by the server.</summary>
        public long? ContentLength { get; set; }

        /// <summary>Content-Type header, including any parameters.</summary>
        public string? ContentType { get; set; }

        /// <summary>Content-Disposition header, when provided by the server.</summary>
        public string? ContentDisposition { get; set; }

        /// <summary>Last-Modified header, when provided by the server.</summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>ETag header, when provided by the server.</summary>
        public string? ETag { get; set; }
    }
}
