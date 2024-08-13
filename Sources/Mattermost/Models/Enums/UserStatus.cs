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
        Away = 1,

        /// <summary>
        /// User is online and active.
        /// </summary>
        Online = 2,

        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 3,

        /// <summary>
        /// Do not disturb status.
        /// </summary>
        DoNotDisturb = 4
    }
}