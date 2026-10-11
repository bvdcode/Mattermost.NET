namespace Mattermost.Models.Enums
{
    /// <summary>Expiration label for a user's custom status.</summary>
    public enum CustomStatusDuration
    {
        /// <summary>The status does not expire.</summary>
        Never = 0,
        /// <summary>Thirty minutes.</summary>
        ThirtyMinutes = 1,
        /// <summary>One hour.</summary>
        OneHour = 2,
        /// <summary>Four hours.</summary>
        FourHours = 3,
        /// <summary>Until the end of today.</summary>
        Today = 4,
        /// <summary>Until the end of this week.</summary>
        ThisWeek = 5,
        /// <summary>Until a specified date and time.</summary>
        DateAndTime = 6
    }
}
