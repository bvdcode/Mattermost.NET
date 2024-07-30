using System;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>
    /// User timezone.
    /// </summary>
    public class Timezone
    {
        /// <summary>
        /// This value is set automatically when the "useAutomaticTimezone" is set to "true".
        /// </summary>
        [JsonPropertyName("automaticTimezone")]
        public string? AutomaticTimezone { get; set; }

        /// <summary>
        /// Value when setting manually the timezone, i.e. "Europe/Berlin".
        /// </summary>
        [JsonPropertyName("manualTimezone")]
        public string? ManualTimezone { get; set; }

        /// <summary>
        /// Set to "true" to use the browser/system timezone, "false" to set manually. Defaults to "true".
        /// </summary>
        [JsonPropertyName("useAutomaticTimezone")]
        public string? UseAutomaticTimezone { get; set; }

        /// <summary>
        /// Current user timezone object.
        /// </summary>
        [JsonIgnore]
        public TimeZoneInfo? TimeZoneInfo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AutomaticTimezone))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(AutomaticTimezone);
                }
                else if (!string.IsNullOrWhiteSpace(ManualTimezone))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(ManualTimezone);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}