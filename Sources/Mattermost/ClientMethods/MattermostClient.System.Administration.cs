using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Helpers;
using Mattermost.Models.Responses;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<UserAudit>> GetServerAuditsAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.ServerAudits + "?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<UserAudit>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<UserAudit>());
        }

        /// <inheritdoc />
        public Task<IList<string>> GetServerLogsAsync(int page = 0, int logsPerPage = MattermostApiLimits.MaxServerLogsPerPage,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (page < 0) { throw new ArgumentOutOfRangeException(nameof(page), "Page cannot be negative."); }
            if (logsPerPage <= 0 || logsPerPage > MattermostApiLimits.MaxServerLogsPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(logsPerPage), $"Page size must be between 1 and {MattermostApiLimits.MaxServerLogsPerPage}.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            string route = Routes.ServerLogs + "?page=" + page.ToString(CultureInfo.InvariantCulture)
                + "&logs_per_page=" + logsPerPage.ToString(CultureInfo.InvariantCulture);
            return SendRequestAsync<IList<string>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<string>());
        }

        /// <inheritdoc />
        public Task<IList<AnalyticsRow>> GetServerAnalyticsAsync(AnalyticsReport report = AnalyticsReport.Standard, string? teamId = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string name = report switch
            {
                AnalyticsReport.Standard => "standard",
                AnalyticsReport.BotPostCountsByDay => "bot_post_counts_day",
                AnalyticsReport.PostCountsByDay => "post_counts_day",
                AnalyticsReport.UserCountsWithPostsByDay => "user_counts_with_posts_day",
                AnalyticsReport.ExtraCounts => "extra_counts",
                _ => throw new ArgumentOutOfRangeException(nameof(report), report, "Unknown analytics report.")
            };
            string route = Routes.ServerAnalytics + "?name=" + name;
            if (teamId is string team) { route += "&team_id=" + EscapeReadIdentifier(team, nameof(teamId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<AnalyticsRow>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<AnalyticsRow>());
        }

        /// <inheritdoc />
        public Task<ServerBusyState> GetServerBusyAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ServerBusyState>(HttpMethod.Get, Routes.ServerBusy, cancellationToken: cancellationToken);
        }
    }
}
