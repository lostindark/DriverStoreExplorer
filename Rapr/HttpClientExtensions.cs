using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rapr
{
    public static class HttpClientExtensions
    {
        public static async Task DownloadAsync(
            this HttpClient client,
            Uri requestUri,
            Stream destination,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            // Get the http headers first to examine the content length
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    // Ignore progress reporting when no progress reporter was passed or when the content length is unknown.
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination).ConfigureAwait(false);
                    }
                    else
                    {
                        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%).
                        var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));

                        // Use extension method to report progress while downloading.
                        await download.CopyToAsync(destination, 65536, relativeProgress, cancellationToken).ConfigureAwait(false);
                        progress.Report(1);
                    }
                }
            }
        }
    }
}
