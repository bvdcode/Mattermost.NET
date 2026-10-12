using Mattermost.Constants;
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
        public Task UpdateSessionDeviceAsync(string? deviceId = null, string? voipDeviceId = null,
            bool? notificationDisabled = null, string? mobileVersion = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> body = new Dictionary<string, string>();
            AddSessionDeviceText(body, "device_id", deviceId, nameof(deviceId));
            AddSessionDeviceText(body, "voip_device_id", voipDeviceId, nameof(voipDeviceId));
            AddSessionDeviceText(body, "mobile_version", mobileVersion, nameof(mobileVersion));
            if (notificationDisabled.HasValue)
            {
                body.Add("device_notification_disabled", notificationDisabled.Value.ToString().ToLowerInvariant());
            }
            if (body.Count == 0) { throw new ArgumentException("At least one session device property is required."); }
            return SendRequestAsync(HttpMethod.Put, Routes.Users + "/sessions/device", body, cancellationToken);
        }

        private static void AddSessionDeviceText(Dictionary<string, string> body, string key, string? value, string parameterName)
        {
            if (value is null) { return; }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("A supplied session device property cannot be blank.", parameterName); }
            body.Add(key, value);
        }
    }
}
