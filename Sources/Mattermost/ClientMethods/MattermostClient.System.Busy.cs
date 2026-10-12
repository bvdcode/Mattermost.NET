using Mattermost.Constants;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SetServerBusyAsync(int seconds = MattermostApiLimits.DefaultServerBusySeconds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (seconds < 1 || seconds > MattermostApiLimits.MaxServerBusySeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Busy duration must be between 1 and 86400 seconds.");
            }
            string route = Routes.ServerBusy + "?seconds=" + seconds.ToString(CultureInfo.InvariantCulture);
            return SendRequestAsync(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task ClearServerBusyAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.ServerBusy, cancellationToken: cancellationToken);
        }
    }
}
