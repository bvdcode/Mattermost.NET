using System;
using System.Collections.Generic;
using System.Text;

namespace Mattermost.Constants
{
    /// <summary>
    /// Mattermost API limits, used to validate the input data.
    /// </summary>
    public static class MattermostApiLimits
    {
        /// <summary>
        /// Maximum length of the post text.
        /// </summary>
        public const int MaxPostMessageLength = 4000;
    }
}
