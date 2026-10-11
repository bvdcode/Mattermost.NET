using Mattermost.Constants;
using Mattermost.Models.Posts;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Post>> GetPostEditHistoryAsync(string postId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Posts + "/" + EscapeReadIdentifier(postId, nameof(postId)) + "/edit_history";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Post>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<Post>());
        }

        /// <inheritdoc />
        public Task<PostLocationInfo> GetPostInfoAsync(string postId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Posts + "/" + EscapeReadIdentifier(postId, nameof(postId)) + "/info";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<PostLocationInfo>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Post> RestorePostVersionAsync(string postId, string restoreVersionId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Posts + "/" + EscapeReadIdentifier(postId, nameof(postId)) + "/restore/"
                + EscapeReadIdentifier(restoreVersionId, nameof(restoreVersionId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Post>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }
    }
}
