using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Users;
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
        public Task<IList<User>> GetUsersWithInvalidEmailsAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (perPage > MattermostApiLimits.MaxUsersPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), $"At most {MattermostApiLimits.MaxUsersPerPage} users can be requested per page.");
            }
            string route = Routes.Users + "/invalid_emails?" + QueryHelpers.BuildPagedQuery(page, perPage);
            return SendRequestAsync<IList<User>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }
    }
}
