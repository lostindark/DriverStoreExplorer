using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rapr
{
    public class UpdateManager : IUpdateManager, IDisposable
    {
        private const string versionInfoUrl = "https://api.github.com/repos/lostindark/DriverStoreExplorer/releases/latest";
        private readonly HttpClient httpClient = new HttpClient();
        private bool disposedValue; // To detect redundant calls

        public async Task<VersionInfo> GetLatestVersionInfo()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

            this.httpClient.DefaultRequestHeaders
                .Add("User-Agent", "System.Net.Http Agent");

            using (var response = await this.httpClient.GetAsync(new Uri(versionInfoUrl)).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var releaseInfo = JObject.Parse(responseBody);

                    var tagName = releaseInfo["tag_name"]?.ToString();
                    var htmlUrl = releaseInfo["html_url"]?.ToString();
                    var downloadUrl = releaseInfo.SelectToken("assets[0].browser_download_url")?.ToObject<string>();

                    if (tagName == null || htmlUrl == null || downloadUrl == null)
                    {
                        return null;
                    }

                    return new VersionInfo
                    {
                        Version = Version.Parse(tagName.TrimStart('v', 'V')),
                        PageUrl = new Uri(htmlUrl),
                        DownloadUrl = new Uri(downloadUrl)
                    };
                }

                return null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.httpClient.Dispose();
                }

                this.disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
