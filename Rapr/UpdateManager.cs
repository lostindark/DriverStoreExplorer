using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

namespace Rapr
{
    public static class UpdateManager
    {
        public static (Version version, string pageUrl, string downloadUrl) GetLatestVersionInfo()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "System.Net.Http Agent");
                using (HttpResponseMessage response = httpClient.GetAsync(new Uri("https://api.github.com/repos/lostindark/DriverStoreExplorer/releases/latest")).GetAwaiter().GetResult())
                {
                    if (response.IsSuccessStatusCode)
                    {
                        JObject releaseInfo = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        Version latestVersion = Version.Parse(releaseInfo["tag_name"].ToString().TrimStart('v', 'V'));
                        string pageUrl = releaseInfo["html_url"].ToString();
                        string downloadUrl = releaseInfo.SelectToken("assets[0].browser_download_url").ToObject<string>();

                        return (latestVersion, pageUrl, downloadUrl);
                    }
                }
            }

            return (null, null, null);
        }
    }
}
