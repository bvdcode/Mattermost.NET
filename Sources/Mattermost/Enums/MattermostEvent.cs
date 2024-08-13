#pragma warning disable CS1591

namespace Mattermost.Enums
{
    public enum MattermostEvent
    {
        Unknown = 0,
        Posted = 1,
        StatusChange = 2,
        Typing = 3,
        MultipleChannelsViewed = 4,
        PreferencesChanged = 5,
        SidebarCategoryUpdated = 6,
        UserAdded = 7,
        EphemeralMessage = 8,
    }
}