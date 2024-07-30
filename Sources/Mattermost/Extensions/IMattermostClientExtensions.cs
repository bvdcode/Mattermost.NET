using Mattermost.Builders;

namespace Mattermost.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IMattermostClient"/>
    /// </summary>
    public static class IMattermostClientExtensions
    {
        /// <summary>
        /// Create post builder.
        /// </summary>
        /// <param name="client"> Mattermost client. </param>
        /// <returns> Created builder. </returns>
        public static PostBuilder CreatePostBuilder(this IMattermostClient client)
        {
            return new PostBuilder(client);
        }
    }
}
