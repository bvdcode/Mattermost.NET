using System.Text.Json.Serialization;

namespace Mattermost.Models.Enums
{
    /// <summary>
    /// User status.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// Unknown status, when status is not recognized.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Away status - user is not active.
        /// </summary>
        [JsonStringEnumMemberName("away")]
        Away = 1,

        /// <summary>
        /// User is online and active.
        /// </summary>
        [JsonStringEnumMemberName("online")]
        Online = 2,

        /// <summary>
        /// User is offline.
        /// </summary>
        [JsonStringEnumMemberName("offline")]
        Offline = 3,

        /// <summary>
        /// Do not disturb status.
        /// </summary>
        [JsonStringEnumMemberName("dnd")]
        DoNotDisturb = 4,

        /// <summary>
        /// User is out of office.
        /// </summary>
        [JsonStringEnumMemberName("ooo")]
        OutOfOffice = 5
    }
}
