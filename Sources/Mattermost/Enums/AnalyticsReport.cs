namespace Mattermost.Enums
{
    /// <summary>Native server analytics reports.</summary>
    public enum AnalyticsReport
    {
        /// <summary>Standard server or team metrics.</summary>
        Standard,

        /// <summary>Bot post counts by day.</summary>
        BotPostCountsByDay,

        /// <summary>Post counts by day.</summary>
        PostCountsByDay,

        /// <summary>Counts of users who posted, grouped by day.</summary>
        UserCountsWithPostsByDay,

        /// <summary>Additional server or team counters.</summary>
        ExtraCounts
    }
}
