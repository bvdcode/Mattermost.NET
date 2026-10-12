using Mattermost.Constants;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<string>> GetAncillaryPermissionsAsync(IEnumerable<string> subsectionPermissions,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            List<string> permissions = PrepareStringBatch(subsectionPermissions, nameof(subsectionPermissions));
            return SendRequestAsync<IList<string>>(HttpMethod.Post, Routes.Permissions + "/ancillary", permissions, cancellationToken);
        }
    }
}
