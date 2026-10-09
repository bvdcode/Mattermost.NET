using Mattermost.Constants;
using Mattermost.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Role>>(HttpMethod.Get, Routes.Roles, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Role> GetRoleAsync(string roleId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(roleId, nameof(roleId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Role>(HttpMethod.Get, Routes.Roles + "/" + id, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Role> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string name = EscapeReadIdentifier(roleName, nameof(roleName));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Role>(HttpMethod.Get, Routes.Roles + "/name/" + name, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            List<string> names = PrepareStringBatch(roleNames, nameof(roleNames)).Distinct(StringComparer.Ordinal).ToList();
            if (names.Count > MattermostApiLimits.MaxRoleNamesPerRequest)
            {
                throw new ArgumentException("A bulk lookup cannot contain more than 100 distinct role names.", nameof(roleNames));
            }
            return SendRequestAsync<IList<Role>>(HttpMethod.Post, Routes.Roles + "/names", names, cancellationToken);
        }
    }
}
