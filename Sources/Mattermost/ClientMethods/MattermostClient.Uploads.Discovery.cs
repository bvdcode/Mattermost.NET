using Mattermost.Constants;
using Mattermost.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<UploadSession>> GetUserUploadsAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UploadSession>>(HttpMethod.Get, Routes.Users + "/" + id + "/uploads",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<UploadSession>());
        }
    }
}
