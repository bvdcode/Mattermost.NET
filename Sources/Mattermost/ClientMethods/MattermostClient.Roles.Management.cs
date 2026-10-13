using Mattermost.Constants;
using Mattermost.Models.Roles;
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
        public Task<Role> UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Roles + "/" + EscapeReadIdentifier(roleId, nameof(roleId)) + "/patch";
            if (permissions is null) { throw new ArgumentNullException(nameof(permissions)); }
            List<string> values = new List<string>();
            foreach (string permission in permissions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(permission)) { throw new ArgumentException("Permission names cannot be blank.", nameof(permissions)); }
                values.Add(permission.Trim());
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Role>(HttpMethod.Put, route, new { permissions = values }, cancellationToken);
        }
    }
}
