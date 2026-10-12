using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<Job> CreateJobAsync(string jobType, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(jobType)) { throw new ArgumentException("A job type is required.", nameof(jobType)); }
            Dictionary<string, string>? snapshot = null;
            if (data is object)
            {
                snapshot = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (KeyValuePair<string, string> property in data)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (property.Value is null) { throw new ArgumentException("Job data values must be strings, not null.", nameof(data)); }
                    snapshot.Add(property.Key, property.Value);
                }
            }
            var body = new { type = jobType.Trim(), data = snapshot };
            return SendRequestAsync<Job>(HttpMethod.Post, Routes.Jobs, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task CancelJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            string route = Routes.Jobs + "/" + EscapeReadIdentifier(jobId, nameof(jobId)) + "/cancel";
            return SendRequestAsync(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateJobStatusAsync(string jobId, JobStatusUpdate status, bool force = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            string route = Routes.Jobs + "/" + EscapeReadIdentifier(jobId, nameof(jobId)) + "/status";
            string statusName;
            switch (status)
            {
                case JobStatusUpdate.Pending: statusName = "pending"; break;
                case JobStatusUpdate.CancelRequested: statusName = "cancel_requested"; break;
                case JobStatusUpdate.Canceled: statusName = "canceled"; break;
                default: throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported job status update.");
            }
            var body = new { status = statusName, force };
            return SendRequestAsync(new HttpMethod("PATCH"), route, body, cancellationToken);
        }
    }
}
