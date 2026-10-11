using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Jobs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Job>> GetJobsAsync(int page = 0, int perPage = 60, string? jobType = null, string? status = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Jobs + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            if (jobType is string type) { route += "&job_type=" + EscapeReadIdentifier(type, nameof(jobType)); }
            if (status is string jobStatus) { route += "&status=" + EscapeReadIdentifier(jobStatus, nameof(status)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Job>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<Job>());
        }

        /// <inheritdoc />
        public Task<Job> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Jobs + "/" + EscapeReadIdentifier(jobId, nameof(jobId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Job>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Job>> GetJobsByTypeAsync(string jobType, int page = 0, int perPage = 60, string? teamId = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Jobs + "/type/" + EscapeReadIdentifier(jobType, nameof(jobType))
                + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            if (teamId is string team) { route += "&team_id=" + EscapeReadIdentifier(team, nameof(teamId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Job>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<Job>());
        }
    }
}
