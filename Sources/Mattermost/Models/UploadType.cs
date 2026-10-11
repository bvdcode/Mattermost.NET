using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>Server upload-session purpose.</summary>
    public enum UploadType
    {
        /// <summary>A file attachment for a channel.</summary>
        [JsonStringEnumMemberName("attachment")]
        Attachment,

        /// <summary>An administrative import upload.</summary>
        [JsonStringEnumMemberName("import")]
        Import
    }
}
