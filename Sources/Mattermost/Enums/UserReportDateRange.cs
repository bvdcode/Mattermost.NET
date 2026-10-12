namespace Mattermost.Enums
{
    /// <summary>Server-calculated date ranges for user post statistics, not account filtering.</summary>
    public enum UserReportDateRange
    {
        /// <summary>All recorded post statistics.</summary>
        AllTime,
        /// <summary>Statistics from the last thirty days.</summary>
        Last30Days,
        /// <summary>Statistics from the previous calendar month, using server-local time.</summary>
        PreviousMonth,
        /// <summary>Statistics from the last six months.</summary>
        Last6Months
    }
}
