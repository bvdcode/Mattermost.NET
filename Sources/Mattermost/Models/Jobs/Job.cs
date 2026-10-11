using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Jobs
{
    /// <summary>A server background job and its current progress.</summary>
    public class Job
    {
        /// <summary>Job identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Native job type.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Job priority.</summary>
        [JsonPropertyName("priority")]
        public long Priority { get; set; }

        /// <summary>Creation time as Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Start time as Unix milliseconds, or zero before the job starts.</summary>
        [JsonPropertyName("start_at")]
        public long StartedAt { get; set; }

        /// <summary>Last activity time as Unix milliseconds.</summary>
        [JsonPropertyName("last_activity_at")]
        public long LastActivityAt { get; set; }

        /// <summary>Native job status.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Progress reported by the job.</summary>
        [JsonPropertyName("progress")]
        public long Progress { get; set; }

        /// <summary>Job-specific string values; may contain sensitive information.</summary>
        [JsonPropertyName("data")]
        public IDictionary<string, string>? Data { get; set; }
    }
}
