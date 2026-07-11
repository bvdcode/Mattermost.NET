using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Date and datetime picker configuration.
    /// </summary>
    public class InteractiveDialogDateTimeConfig
    {
        /// <summary>
        /// Earliest selectable date.
        /// </summary>
        [JsonPropertyName("min_date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MinDate { get; set; }

        /// <summary>
        /// Latest selectable date.
        /// </summary>
        [JsonPropertyName("max_date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxDate { get; set; }

        /// <summary>
        /// Time selection interval in minutes.
        /// </summary>
        [JsonPropertyName("time_interval")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TimeInterval { get; set; }

        /// <summary>
        /// IANA timezone used to display and submit the time.
        /// </summary>
        [JsonPropertyName("location_timezone")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocationTimezone { get; set; }

        /// <summary>
        /// Whether users can type the time manually.
        /// </summary>
        [JsonPropertyName("manual_time_entry")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ManualTimeEntry { get; set; }

        /// <summary>
        /// Deprecated manual time entry setting accepted by older servers.
        /// </summary>
        [JsonPropertyName("allow_manual_time_entry")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowManualTimeEntry { get; set; }
    }
}
