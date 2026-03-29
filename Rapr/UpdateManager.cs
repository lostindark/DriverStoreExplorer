using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
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
                    var digest = releaseInfo.SelectToken("assets[0].digest")?.ToObject<string>();

                    if (tagName == null || htmlUrl == null || downloadUrl == null)
                    {
                        return null;
                    }

                    // Parse "sha256:<hex>" format
                    string sha256 = null;
                    if (digest != null && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                    {
                        sha256 = digest.Substring("sha256:".Length);
                    }

                    return new VersionInfo
                    {
                        Version = Version.Parse(tagName.TrimStart('v', 'V')),
                        PageUrl = new Uri(htmlUrl),
                        DownloadUrl = new Uri(downloadUrl),
                        Sha256 = sha256
                    };
                }

                return null;
            }
        }

        public async Task ApplyUpdateAsync(VersionInfo versionInfo, IProgress<float> progress)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException(nameof(versionInfo));
            }

            if (!IsGitHubUrl(versionInfo.DownloadUrl))
            {
                throw new InvalidOperationException("Download URL is not from a trusted GitHub domain.");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string tempBaseDir = Path.Combine(Path.GetTempPath(), "DriverStoreExplorer");
            string downloadFileName = Path.GetFileName(versionInfo.DownloadUrl.LocalPath);
            string tempZipPath = Path.Combine(tempBaseDir, downloadFileName);
            string tempExtractPath = Path.Combine(tempBaseDir, "Update");

            if (!Directory.Exists(tempBaseDir))
            {
                Directory.CreateDirectory(tempBaseDir);
            }

            // Clean up any previous update artifacts
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }

            if (Directory.Exists(tempExtractPath))
            {
                Directory.Delete(tempExtractPath, true);
            }

            // Download the zip
            using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await this.httpClient.DownloadAsync(versionInfo.DownloadUrl, fileStream, progress).ConfigureAwait(false);
            }

            // Verify SHA256 hash
            if (!string.IsNullOrEmpty(versionInfo.Sha256))
            {
                using (var sha256 = SHA256.Create())
                using (var fileStream = new FileStream(tempZipPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var hashBytes = sha256.ComputeHash(fileStream);
                    var actualHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                    if (!actualHash.Equals(versionInfo.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(tempZipPath);
                        throw new InvalidOperationException("SHA256 hash of the downloaded file does not match the expected value.");
                    }
                }
            }

            // Extract zip
            ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

            // Find the actual content directory (zip may have a single root folder)
            string sourceDir = tempExtractPath;
            var subDirs = Directory.GetDirectories(tempExtractPath);
            if (subDirs.Length == 1 && Directory.GetFiles(tempExtractPath).Length == 0)
            {
                sourceDir = subDirs[0];
            }

            string appDir = Path.GetFullPath(DSEFormHelper.GetApplicationFolder());
            string currentExePath = Assembly.GetExecutingAssembly().Location;

            // Validate all extracted file paths before making any changes
            var filesToCopy = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var file in filesToCopy)
            {
                string relativePath = file.Substring(sourceDir.Length + 1);
                string destPath = Path.GetFullPath(Path.Combine(appDir, relativePath));

                if (!destPath.StartsWith(appDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                    && !destPath.Equals(appDir, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Update package contains a file that escapes the application directory: {relativePath}");
                }
            }

            // Rename the running exe — Windows allows renaming a running executable
            string oldExePath = currentExePath + ".old";
            if (File.Exists(oldExePath))
            {
                File.Delete(oldExePath);
            }

            File.Move(currentExePath, oldExePath);

            try
            {
                // Copy all files from extracted folder to app directory
                foreach (var file in filesToCopy)
                {
                    string relativePath = file.Substring(sourceDir.Length + 1);
                    string destPath = Path.Combine(appDir, relativePath);
                    string destDir = Path.GetDirectoryName(destPath);

                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(file, destPath, overwrite: true);
                }
            }
            catch
            {
                // Rollback: restore the original exe
                if (File.Exists(currentExePath))
                {
                    File.Delete(currentExePath);
                }

                File.Move(oldExePath, currentExePath);
                throw;
            }

            // Clean up temp folder
            try { Directory.Delete(tempBaseDir, true); } catch { }
        }

        private static bool IsGitHubUrl(Uri url)
        {
            return url.Scheme == Uri.UriSchemeHttps
                && (url.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
                    || url.Host.EndsWith(".github.com", StringComparison.OrdinalIgnoreCase)
                    || url.Host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Deletes leftover .old files from a previous update.
        /// </summary>
        public static void CleanUpOldFiles()
        {
            try
            {
                string appDir = DSEFormHelper.GetApplicationFolder();

                foreach (var oldFile in Directory.GetFiles(appDir, "*.old"))
                {
                    try
                    {
                        File.Delete(oldFile);
                    }
                    catch
                    {
                        // File may still be locked if the previous instance hasn't fully exited
                    }
                }
            }
            catch
            {
                // Non-critical — will retry on next launch
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
