using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Jobs;
using Mattermost.Models.Plugins;
using Mattermost.Models.Responses;
using Mattermost.Models.Roles;
using Mattermost.Models.Users;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Server health, client settings, administration, roles, jobs and plugin metadata.
    /// </summary>
    public interface IMattermostSystemClient
    {
        /// <summary>Read the server configuration visible to the current account.</summary>
        /// <param name="removeMasked">Omit masked values. Requires Mattermost 10.4 or later.</param>
        /// <param name="removeDefaults">Omit default values. Requires Mattermost 10.4 or later.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Native configuration sections, preserving field names and JSON value types.</returns>
        /// <remarks>Requires a system console read permission; the server filters sections by permission and masks secrets. Configuration may still contain sensitive data. Does not change settings.</remarks>
        Task<IDictionary<string, JsonElement>> GetServerConfigAsync(bool removeMasked = false, bool removeDefaults = false,
            CancellationToken cancellationToken = default);

        /// <summary>Read which configuration fields are overridden by environment variables. Requires Mattermost 4.10 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Permission-filtered native sections containing override flags, not the environment variable values. An empty or null server result becomes an empty dictionary.</returns>
        Task<IDictionary<string, JsonElement>> GetEnvironmentConfigAsync(CancellationToken cancellationToken = default);

        /// <summary>Read the applied database schema migrations.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires a system console read permission. Does not run migrations or modify the database.</remarks>
        Task<IList<AppliedSchemaMigration>> GetAppliedSchemaMigrationsAsync(CancellationToken cancellationToken = default);

        /// <summary>Read whether the first administrator has completed initial server setup.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Does not complete setup or install plugins.</remarks>
        Task<bool> GetFirstAdminOnboardingCompleteAsync(CancellationToken cancellationToken = default);

        /// <summary>Read information about the latest Mattermost server release.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system and a non-restricted administrator session. The server fetches release metadata; this does not upgrade or restart the server.</remarks>
        Task<ServerRelease> GetLatestServerReleaseAsync(CancellationToken cancellationToken = default);

        /// <summary>Get a page of audit entries for all users.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires read_audits. Entries may include private addresses, session identifiers and event details.</remarks>
        Task<IList<UserAudit>> GetServerAuditsAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get a page of server log lines.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="logsPerPage">Between one and MaxServerLogsPerPage lines.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires get_logs and a non-restricted administrator session. Logs may contain sensitive data.</remarks>
        Task<IList<string>> GetServerLogsAsync(int page = 0, int logsPerPage = MattermostApiLimits.MaxServerLogsPerPage,
            CancellationToken cancellationToken = default);

        /// <summary>Get a server analytics report using the native name/value format. Requires Mattermost 4.0 or later.</summary>
        /// <param name="report">Report to retrieve.</param>
        /// <param name="teamId">Optional team filter; null requests server-wide data.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires get_analytics. Names and values depend on the requested report.</remarks>
        Task<IList<AnalyticsRow>> GetServerAnalyticsAsync(AnalyticsReport report = AnalyticsReport.Standard, string? teamId = null,
            CancellationToken cancellationToken = default);

        /// <summary>Read the server's busy flag and expiry. Requires Mattermost 5.20 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Does not set or clear the flag.</remarks>
        Task<ServerBusyState> GetServerBusyAsync(CancellationToken cancellationToken = default);

        /// <summary>Get a page of background jobs readable by the current session. Requires Mattermost 4.1 or later.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="jobType">Optional native job type; null includes all readable types.</param>
        /// <param name="status">Optional native job status; null includes all statuses.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Permissions depend on job type. Does not accept a team filter.</remarks>
        Task<IList<Job>> GetJobsAsync(int page = 0, int perPage = 60, string? jobType = null, string? status = null,
            CancellationToken cancellationToken = default);

        /// <summary>Get a background job by identifier. Requires Mattermost 4.1 or later.</summary>
        /// <param name="jobId">Job identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires permission to read the job's type.</remarks>
        Task<Job> GetJobAsync(string jobId, CancellationToken cancellationToken = default);

        /// <summary>Get a page of background jobs of one native type. Requires Mattermost 4.1 or later.</summary>
        /// <param name="jobType">Native job type.</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="teamId">Optional team filter on servers supporting team-scoped jobs.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires permission to read the job type. Team administrators may read access_control_sync jobs for their team with manage_team_access_rules.</remarks>
        Task<IList<Job>> GetJobsByTypeAsync(string jobType, int page = 0, int perPage = 60, string? teamId = null,
            CancellationToken cancellationToken = default);

        /// <summary>Read installed plugin manifests grouped by activation state. Requires Mattermost 4.4 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires enabled plugins and sysconsole_read_plugins. Does not return current plugin configuration values.</remarks>
        Task<PluginsResponse> GetPluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>Read active plugins with web application bundles. Requires Mattermost 4.4 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires enabled plugins. Server-side executable and settings-schema data are omitted by the server.</remarks>
        Task<IList<PluginManifest>> GetWebappPluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>Read cluster-aware installed plugin runtime states. Requires Mattermost 4.4 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires enabled plugins and sysconsole_read_plugins. Error details and server paths may be sensitive.</remarks>
        Task<IList<PluginStatus>> GetPluginStatusesAsync(CancellationToken cancellationToken = default);

        /// <summary>Read Marketplace plugin metadata. Requires Mattermost 5.16 or later.</summary>
        /// <param name="filter">Optional search text; empty searches all plugins.</param>
        /// <param name="localOnly">Read local metadata without fetching the remote Marketplace.</param>
        /// <param name="remoteOnly">Read only remote metadata; mutually exclusive with localOnly.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires enabled plugins and Marketplace. Unless remoteOnly is true, requires sysconsole_read_plugins. Returns the native matching list without client-side paging. No plugin is installed or activated.</remarks>
        Task<IList<MarketplacePlugin>> GetMarketplacePluginsAsync(string? filter = null, bool localOnly = false,
            bool remoteOnly = false, CancellationToken cancellationToken = default);

        /// <summary>Read whether a system administrator has visited the Marketplace. Requires Mattermost 5.33 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Does not mark the Marketplace as visited.</remarks>
        Task<bool> GetMarketplaceVisitedByAdminAsync(CancellationToken cancellationToken = default);

        /// <summary>Get all roles. Requires Mattermost 5.33 or later and manage_system permission.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Role>> GetRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>Get a role by identifier. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleId">Role identifier; surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Role> GetRoleAsync(string roleId, CancellationToken cancellationToken = default);

        /// <summary>Get a role by name. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleName">Role name; surrounding whitespace is removed and case is preserved.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Role> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>Get roles by name. Requires Mattermost 4.9 or later and authentication.</summary>
        /// <param name="roleNames">Nonempty collection of up to 100 distinct names; whitespace and duplicate names are removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching roles. Input order is not preserved; missing names may be omitted.</returns>
        Task<IList<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

        /// <summary>Check server health without authentication. Requires Mattermost 3.10 or later.</summary>
        /// <param name="checkBackends">Ask the server to test database and file-storage connectivity.</param>
        /// <param name="useRestSemantics">Return an unhealthy status as data instead of an HTTP error. Requires Mattermost 9.6 or later when true.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>Server status, optional backend statuses, and mobile client requirements.</returns>
        /// <remarks>By default, an unhealthy server returns HTTP 500 and throws MattermostClientException. Backend checks perform test writes on the server. No login or user-profile request is made.</remarks>
        Task<SystemStatusResponse> GetSystemStatusAsync(bool checkBackends = false, bool useRestSemantics = false,
            CancellationToken cancellationToken = default);

        /// <summary>Get server-supported timezone names. Requires authentication and Mattermost 3.10 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Timezone names, such as Europe/Berlin.</returns>
        Task<IList<string>> GetSupportedTimezonesAsync(CancellationToken cancellationToken = default);

        /// <summary>Get the configuration subset exposed to Mattermost clients. Does not change server settings.</summary>
        /// <param name="authenticated">Use the current account to request additional client settings. False makes no login request and does not send the saved token.</param>
        /// <param name="cancellationToken">Cancels the request and, when requested, authentication.</param>
        /// <returns>Native string values, including booleans and numbers encoded as strings. Keys vary with server version and configuration.</returns>
        Task<IDictionary<string, string>> GetClientConfigAsync(bool authenticated = false, CancellationToken cancellationToken = default);

        /// <summary>Get the license subset exposed to Mattermost clients. Does not activate or change a license.</summary>
        /// <param name="authenticated">Use the current account. Additional information requires read_license_information permission. False makes no login request and does not send the saved token.</param>
        /// <param name="cancellationToken">Cancels the request and, when requested, authentication.</param>
        /// <returns>Native string values. The server determines which license fields are visible.</returns>
        Task<IDictionary<string, string>> GetClientLicenseAsync(bool authenticated = false, CancellationToken cancellationToken = default);

        /// <summary>Get the monthly active user load relative to the licensed user limit. Requires authentication and Mattermost 10.8 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Ratio multiplied by 1000 and rounded: 1000 represents 100 percent. Returns zero when no license or licensed user limit is present. Bot and deleted accounts are excluded.</returns>
        Task<long> GetLicenseLoadMetricAsync(CancellationToken cancellationToken = default);
    }
}
