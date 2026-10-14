namespace Mattermost.Models.Groups
{
    /// <summary>Optional filters for the group list.</summary>
    public class GroupSearchOptions
    {
        /// <summary>Native search term, trimmed before sending.</summary>
        public string? Query { get; set; }

        /// <summary>Include membership counts.</summary>
        public bool IncludeMemberCount { get; set; }

        /// <summary>Return only mentionable groups.</summary>
        public bool FilterAllowReference { get; set; }

        /// <summary>Exclude groups already linked to this team.</summary>
        public string? NotAssociatedToTeam { get; set; }

        /// <summary>Exclude groups already linked to this channel.</summary>
        public string? NotAssociatedToChannel { get; set; }

        /// <summary>Return groups updated since this Unix millisecond time.</summary>
        public long Since { get; set; }

        /// <summary>Include archived groups in addition to active groups.</summary>
        public bool IncludeArchived { get; set; }
    }
}
