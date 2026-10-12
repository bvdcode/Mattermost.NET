using Mattermost.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task NotifyAdminAsync(string requiredPlan, string requiredFeature, bool trialNotification = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (requiredPlan is null) { throw new ArgumentNullException(nameof(requiredPlan)); }
            if (requiredFeature is null || string.IsNullOrWhiteSpace(requiredFeature))
            {
                throw new ArgumentException("Required feature cannot be blank.", nameof(requiredFeature));
            }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                required_plan = requiredPlan.Trim(),
                required_feature = requiredFeature.Trim(),
                trial_notification = trialNotification
            };
            return SendRequestAsync(HttpMethod.Post, Routes.Users + "/notify-admin", body, cancellationToken);
        }
    }
}
