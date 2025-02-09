using System;
using Mattermost.Enums;
using System.Threading.Tasks;
using Mattermost.Models.Posts;
using System.Collections.Generic;

namespace Mattermost.Builders
{
    /// <summary>
    /// Mattermost post builder.
    /// </summary>
    public class PostBuilder
    {
        private string text;
        private string channelId;
        private string replyToPostId;
        private readonly List<string> files;
        private MessagePriority messagePriority;

        /// <summary>
        /// Create new post builder.
        /// </summary>
        public PostBuilder()
        {
            text = string.Empty;
            replyToPostId = string.Empty;
            files = new List<string>();
            channelId = string.Empty;
            messagePriority = MessagePriority.Empty;
        }

        /// <summary>
        /// Send post to specified channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> Current builder. </returns>
        /// <exception cref="ArgumentException"> If channel identifier is empty or null. </exception>
        public PostBuilder ToChannel(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("Channel identifier cannot be empty.", nameof(channelId));
            }
            this.channelId = channelId;
            return this;
        }

        /// <summary>
        /// Set post text message.
        /// </summary>
        /// <param name="text"> Text message (markdown supported). </param>
        /// <returns> Current builder. </returns>
        public PostBuilder AddText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                this.text = string.Empty;
                return this;
            }
            this.text = text;
            return this;
        }

        /// <summary>
        /// Reply to specified root post identifier.
        /// </summary>
        /// <param name="rootPostId"> Post identifier. </param>
        /// <returns> Current builder. </returns>
        public PostBuilder ReplyToPostId(string rootPostId)
        {
            if (string.IsNullOrWhiteSpace(rootPostId))
            {
                replyToPostId = string.Empty;
                return this;
            }
            replyToPostId = rootPostId;
            return this;
        }

        /// <summary>
        /// Set post priority.
        /// </summary>
        /// <param name="messagePriority"> Post priority. </param>
        /// <returns> Current buidler. </returns>
        public PostBuilder SetPriority(MessagePriority messagePriority)
        {
            this.messagePriority = messagePriority;
            return this;
        }

        /// <summary>
        /// Attach file to post by file identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> Current builder. </returns>
        /// <exception cref="ArgumentNullException"> If file identifier is empty. </exception>
        public PostBuilder AddFile(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentNullException(nameof(fileId), "File identifier cannot be empty.");
            }
            if (files.Contains(fileId))
            {
                return this;
            }
            files.Add(fileId);
            return this;
        }

        /// <summary>
        /// Sent completed post to specified channel.
        /// </summary>
        /// <returns> Created post. </returns>
        /// <exception cref="ArgumentException"></exception>
        public Task<Post> SendMessageAsync(IMattermostClient client)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("Channel identifier cannot be empty.", nameof(channelId));
            }
            if (string.IsNullOrWhiteSpace(text) && files.Count == 0)
            {
                throw new ArgumentException("Post cannot be empty - no text and no files there.");
            }
            return client.CreatePostAsync(channelId, text, replyToPostId, messagePriority, files.Count > 0 ? files : null);
        }
    }
}