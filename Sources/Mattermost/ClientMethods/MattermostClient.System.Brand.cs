using Mattermost.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task SetBrandImageAsync(byte[] image, string fileName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            return SetImageAsync(Routes.BrandImage, image, fileName, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteBrandImageAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.BrandImage, cancellationToken: cancellationToken);
        }
    }
}
