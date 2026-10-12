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
        public Task<IList<string>> GetImportsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, Routes.Imports, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteImportAsync(string importName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(importName) || importName.Length <= 4
                || !importName.EndsWith(".zip", StringComparison.Ordinal) || importName.IndexOf('/') >= 0 || importName.IndexOf('\\') >= 0)
            {
                throw new ArgumentException("An import name must be a single .zip filename returned by the server.", nameof(importName));
            }
            foreach (char character in importName)
            {
                if (char.IsControl(character)) { throw new ArgumentException("An import name cannot contain control characters.", nameof(importName)); }
            }
            return SendRequestAsync(HttpMethod.Delete, Routes.Imports + "/" + Uri.EscapeDataString(importName), cancellationToken: cancellationToken);
        }
    }
}
