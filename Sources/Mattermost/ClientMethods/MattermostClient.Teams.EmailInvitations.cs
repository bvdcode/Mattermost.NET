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
        public Task InviteUsersToTeamAsync(string teamId, IEnumerable<string> emails, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Teams + "/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/invite/email";
            List<string> values = PrepareStringBatch(emails, nameof(emails));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, route, new { emails = values }, cancellationToken);
        }
    }
}
