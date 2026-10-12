namespace Mattermost.Enums
{
    /// <summary>Guest-only filters for administrative user reports.</summary>
    public enum UserReportGuestFilter
    {
        /// <summary>All guest accounts, regardless of channel count.</summary>
        AllGuests,
        /// <summary>Guests belonging to exactly one active public or private channel.</summary>
        SingleChannelGuests,
        /// <summary>Guests belonging to more than one active public or private channel.</summary>
        MultiChannelGuests
    }
}
