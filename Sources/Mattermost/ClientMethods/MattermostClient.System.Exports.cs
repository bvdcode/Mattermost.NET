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
        public Task<IList<string>> GetExportsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, Routes.Exports, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<byte[]> DownloadExportAsync(string exportName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return GetBinaryContentAsync(GetExportRoute(exportName), cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteExportAsync(string exportName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, GetExportRoute(exportName), cancellationToken: cancellationToken);
        }

        private static string GetExportRoute(string exportName)
        {
            if (string.IsNullOrWhiteSpace(exportName) || exportName == "." || exportName == ".."
                || exportName.IndexOf('/') >= 0 || exportName.IndexOf('\\') >= 0)
            {
                throw new ArgumentException("An export name must be a single filename returned by the server.", nameof(exportName));
            }
            foreach (char character in exportName)
            {
                if (char.IsControl(character)) { throw new ArgumentException("An export name cannot contain control characters.", nameof(exportName)); }
            }
            return Routes.Exports + "/" + Uri.EscapeDataString(exportName);
        }
    }
}
