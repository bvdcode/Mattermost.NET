using System.Collections.Generic;

namespace Mattermost.Models.Users
{
    /// <summary>Complete editable user profile for replacement. Use UserPatch to change individual fields.</summary>
    public class UserProfileUpdate
    {
        /// <summary>Required username.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Required email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Required locale.</summary>
        public string Locale { get; set; } = string.Empty;

        /// <summary>Current password required when changing your own email. Does not set a new password.</summary>
        public string? Password { get; set; }

        /// <summary>Nickname; null or empty clears it.</summary>
        public string? Nickname { get; set; }

        /// <summary>First name; null or empty clears it.</summary>
        public string? FirstName { get; set; }

        /// <summary>Last name; null or empty clears it.</summary>
        public string? LastName { get; set; }

        /// <summary>Position; null or empty clears it.</summary>
        public string? Position { get; set; }

        /// <summary>Complete timezone settings; null clears the stored map.</summary>
        public Timezone? Timezone { get; set; }

        /// <summary>Complete profile properties; null or empty clears the stored map.</summary>
        public IDictionary<string, string>? Props { get; set; }

        /// <summary>Complete notification properties; null or empty resets the native defaults.</summary>
        public IDictionary<string, string>? NotifyProps { get; set; }

        /// <summary>Whether the user opts into marketing. Always sent, including false.</summary>
        public bool AllowMarketing { get; set; }
    }
}
