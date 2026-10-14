using Mattermost.Constants;
using Mattermost.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public async Task SubmitPerformanceReportAsync(PerformanceReport report, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (report is null) { throw new ArgumentNullException(nameof(report)); }
            if (string.IsNullOrWhiteSpace(report.Version)) { throw new ArgumentException("A report version is required.", nameof(report)); }
            if (IsNonFinite(report.Start) || IsNonFinite(report.End) || report.Start > report.End)
            {
                throw new ArgumentException("Report timestamps must be finite and ordered.", nameof(report));
            }
            ValidateMetricSamples(report.Counters, cancellationToken);
            ValidateMetricSamples(report.Histograms, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement snapshot = JsonSerializer.SerializeToElement(report);
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Post, Routes.ClientPerformance,
                snapshot, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw CreateRequestException(response, json, HttpMethod.Post, Routes.ClientPerformance);
            }
        }

        private static void ValidateMetricSamples(IList<MetricSample>? samples, CancellationToken cancellationToken)
        {
            if (samples is null) { return; }
            foreach (MetricSample sample in samples)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (sample is null || string.IsNullOrWhiteSpace(sample.Metric) || IsNonFinite(sample.Value))
                {
                    throw new ArgumentException("Metric samples require a name and a finite value.", nameof(samples));
                }
            }
        }

        private static bool IsNonFinite(double value) => double.IsNaN(value) || double.IsInfinity(value);
    }
}
