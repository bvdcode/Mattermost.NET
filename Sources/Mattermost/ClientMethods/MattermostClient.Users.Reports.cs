using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<UserReport>> GetUserReportsAsync(int pageSize = 50, UserReportSortColumn sortColumn = UserReportSortColumn.Username,
            bool descending = false, string? fromId = null, string? fromColumnValue = null, bool previousPage = false,
            UserReportDateRange dateRange = UserReportDateRange.AllTime, string? teamId = null, string? role = null,
            bool hasNoTeam = false, bool hideActive = false, bool hideInactive = false, string? searchTerm = null,
            UserReportGuestFilter? guestFilter = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (pageSize < 1 || pageSize > 100) { throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100."); }
            if ((fromId is null) != (fromColumnValue is null))
            {
                throw new ArgumentException("A cursor requires both a user identifier and its sorted-column value.", nameof(fromId));
            }
            string query = BuildUserReportFilters(teamId, role, hasNoTeam, hideActive, hideInactive, searchTerm, guestFilter)
                + "&page_size=" + pageSize.ToString(CultureInfo.InvariantCulture)
                + "&sort_column=" + GetUserReportSortColumn(sortColumn)
                + "&sort_direction=" + (descending ? "desc" : "asc")
                + "&direction=" + (previousPage ? "prev" : "next")
                + "&date_range=" + GetUserReportDateRange(dateRange);
            if (fromId is string id)
            {
                if (string.IsNullOrEmpty(fromColumnValue))
                {
                    throw new ArgumentException("The server requires a nonempty sorted-column value to apply a cursor.", nameof(fromColumnValue));
                }
                query += "&from_id=" + EscapeReadIdentifier(id, nameof(fromId)) + "&from_column_value=" + Uri.EscapeDataString(fromColumnValue);
            }
            return SendRequestAsync<IList<UserReport>>(HttpMethod.Get, Routes.UserReports + "?" + query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> GetUserReportCountAsync(string? teamId = null, string? role = null, bool hasNoTeam = false,
            bool hideActive = false, bool hideInactive = false, string? searchTerm = null,
            UserReportGuestFilter? guestFilter = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            string query = BuildUserReportFilters(teamId, role, hasNoTeam, hideActive, hideInactive, searchTerm, guestFilter);
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.UserReports + "/count?" + query,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Number || !response.TryGetInt64(out long count))
            {
                throw new JsonException("The report count must be a valid 64-bit integer.");
            }
            return count;
        }

        private static string BuildUserReportFilters(string? teamId, string? role, bool hasNoTeam, bool hideActive,
            bool hideInactive, string? searchTerm, UserReportGuestFilter? guestFilter)
        {
            if (hideActive && hideInactive) { throw new ArgumentException("Active and inactive users cannot both be hidden.", nameof(hideInactive)); }
            string query = "has_no_team=" + hasNoTeam.ToString().ToLowerInvariant() + "&hide_active=" + hideActive.ToString().ToLowerInvariant()
                + "&hide_inactive=" + hideInactive.ToString().ToLowerInvariant();
            if (teamId is string team) { query += "&team_filter=" + EscapeReadIdentifier(team, nameof(teamId)); }
            if (role is string roleName) { query += "&role_filter=" + Uri.EscapeDataString(roleName.Trim()); }
            if (searchTerm is string term) { query += "&search_term=" + Uri.EscapeDataString(term.Trim()); }
            if (guestFilter is UserReportGuestFilter guest) { query += "&guest_filter=" + GetUserReportGuestFilter(guest); }
            return query;
        }

        private static string GetUserReportSortColumn(UserReportSortColumn value)
        {
            switch (value)
            {
                case UserReportSortColumn.Username: return "Username";
                case UserReportSortColumn.CreateAt: return "CreateAt";
                case UserReportSortColumn.FirstName: return "FirstName";
                case UserReportSortColumn.LastName: return "LastName";
                case UserReportSortColumn.Nickname: return "Nickname";
                case UserReportSortColumn.Email: return "Email";
                case UserReportSortColumn.Roles: return "Roles";
                default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported user-report sort column.");
            }
        }

        private static string GetUserReportDateRange(UserReportDateRange value)
        {
            switch (value)
            {
                case UserReportDateRange.AllTime: return "all_time";
                case UserReportDateRange.Last30Days: return "last_30_days";
                case UserReportDateRange.PreviousMonth: return "previous_month";
                case UserReportDateRange.Last6Months: return "last_6_months";
                default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported user-report date range.");
            }
        }

        private static string GetUserReportGuestFilter(UserReportGuestFilter value)
        {
            switch (value)
            {
                case UserReportGuestFilter.AllGuests: return "all";
                case UserReportGuestFilter.SingleChannelGuests: return "single_channel";
                case UserReportGuestFilter.MultiChannelGuests: return "multi_channel";
                default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported user-report guest filter.");
            }
        }
    }
}
