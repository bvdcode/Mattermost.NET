using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Mattermost.Models.Enums;

namespace Mattermost.Models.Responses.Websocket.Users
{
    /// <summary>
    /// User status update.
    /// </summary>
    public class UserStatusUpdate
    {
        /// <summary>
        /// New user status.
        /// </summary>
        [JsonPropertyName("status")]
        public string StatusText { get; set; } = string.Empty;

        /// <summary>
        /// User new status.
        /// </summary>
        [JsonIgnore]
        public UserStatus Status => GetStatus(StatusText);

        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        private UserStatus GetStatus(string statusText)
        {
            return statusText.ToLower() switch
            {
                "online" => UserStatus.Online,
                "offline" => UserStatus.Offline,
                "away" => UserStatus.Away,
                "dnd" => UserStatus.DoNotDisturb,
                _ => UserStatus.Unknown,
            };
        }
    }
}
