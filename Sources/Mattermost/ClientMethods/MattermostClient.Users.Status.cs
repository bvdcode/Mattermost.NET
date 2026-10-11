using Mattermost.Constants;
using Mattermost.Models.Enums;
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
        public async Task<UserPresence> UpdateUserStatusAsync(string userId, UserStatus status, long dndEndTime = 0,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            string statusName;
            switch (status)
            {
                case UserStatus.Online: statusName = "online"; break;
                case UserStatus.Offline: statusName = "offline"; break;
                case UserStatus.Away: statusName = "away"; break;
                case UserStatus.DoNotDisturb: statusName = "dnd"; break;
                case UserStatus.Unknown:
                case UserStatus.OutOfOffice:
                default: throw new ArgumentOutOfRangeException(nameof(status), "Only online, offline, away and do not disturb can be set through this endpoint.");
            }
            if (dndEndTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dndEndTime), "The expiry cannot be negative.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            string actualUserId = userId.Trim();
            if (actualUserId == "me")
            {
                await CheckAuthorizedAsync(cancellationToken).ConfigureAwait(false);
                actualUserId = CurrentUserInfo.Id;
            }
            var body = new { user_id = actualUserId, status = statusName, dnd_end_time = dndEndTime };
            return await SendRequestAsync<UserPresence>(HttpMethod.Put, Routes.Users + "/" + id + "/status", body,
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task UpdateUserCustomStatusAsync(string userId, string emoji, string text,
            CustomStatusDuration duration = CustomStatusDuration.Never, DateTimeOffset? expiresAt = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildCustomStatusBody(emoji, text, duration, expiresAt);
            return SendRequestAsync(HttpMethod.Put, Routes.Users + "/" + id + "/status/custom", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task ClearUserCustomStatusAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.Users + "/" + id + "/status/custom",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteRecentUserCustomStatusAsync(string userId, string emoji, string text,
            CustomStatusDuration duration = CustomStatusDuration.Never, DateTimeOffset? expiresAt = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildCustomStatusBody(emoji, text, duration, expiresAt);
            return SendRequestAsync(HttpMethod.Post, Routes.Users + "/" + id + "/status/custom/recent/delete", body,
                cancellationToken);
        }

        private static Dictionary<string, object> BuildCustomStatusBody(string emoji, string text,
            CustomStatusDuration duration, DateTimeOffset? expiresAt)
        {
            if (emoji is null) { throw new ArgumentNullException(nameof(emoji)); }
            if (text is null) { throw new ArgumentNullException(nameof(text)); }
            if (emoji.Length == 0 && text.Length == 0)
            {
                throw new ArgumentException("A custom status requires an emoji or text.", nameof(text));
            }
            string durationName;
            switch (duration)
            {
                case CustomStatusDuration.Never: durationName = string.Empty; break;
                case CustomStatusDuration.ThirtyMinutes: durationName = "thirty_minutes"; break;
                case CustomStatusDuration.OneHour: durationName = "one_hour"; break;
                case CustomStatusDuration.FourHours: durationName = "four_hours"; break;
                case CustomStatusDuration.Today: durationName = "today"; break;
                case CustomStatusDuration.ThisWeek: durationName = "this_week"; break;
                case CustomStatusDuration.DateAndTime: durationName = "date_and_time"; break;
                default: throw new ArgumentOutOfRangeException(nameof(duration));
            }
            if (duration == CustomStatusDuration.Never)
            {
                if (expiresAt is DateTimeOffset expiry && expiry != DateTimeOffset.MinValue)
                {
                    throw new ArgumentException("A status without expiration cannot specify an expiry.", nameof(expiresAt));
                }
            }
            else if (expiresAt is null || expiresAt == DateTimeOffset.MinValue)
            {
                throw new ArgumentException("An expiring status requires an explicit expiry.", nameof(expiresAt));
            }
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["emoji"] = emoji, ["text"] = text, ["duration"] = durationName
            };
            if (expiresAt is DateTimeOffset expiration)
            {
                body["expires_at"] = expiration.ToUniversalTime();
            }
            return body;
        }
    }
}
