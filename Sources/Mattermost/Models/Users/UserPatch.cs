using System.Collections.Generic;

namespace Mattermost.Models.Users
{
    /// <summary>Profile fields to change. Null leaves a field unchanged; empty text clears optional text fields.</summary>
    public class UserPatch
    {
        /// <summary>New username.</summary>
        public string? Username { get; set; }

        /// <summary>New email address.</summary>
        public string? Email { get; set; }

        /// <summary>Current password required when changing your own email. Does not change the password.</summary>
        public string? Password { get; set; }

        /// <summary>Nickname; empty clears it.</summary>
        public string? Nickname { get; set; }

        /// <summary>First name; empty clears it.</summary>
        public string? FirstName { get; set; }

        /// <summary>Last name; empty clears it.</summary>
        public string? LastName { get; set; }

        /// <summary>Position; empty clears it.</summary>
        public string? Position { get; set; }

        /// <summary>Preferred locale.</summary>
        public string? Locale { get; set; }

        /// <summary>Complete timezone settings; supplied settings replace the previous map.</summary>
        public Timezone? Timezone { get; set; }

        /// <summary>Complete profile properties; an empty map clears them.</summary>
        public IDictionary<string, string>? Props { get; set; }

        /// <summary>Complete native notification properties; supplied properties replace the previous map.</summary>
        public IDictionary<string, string>? NotifyProps { get; set; }
    }
}
