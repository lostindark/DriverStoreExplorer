using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool HandlesRestart => false;

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

            InstallUpdate(sourceDir, Assembly.GetExecutingAssembly().Location);

            // Clean up temp folder
            try { Directory.Delete(tempBaseDir, true); } catch { }
        }

        private static void InstallUpdate(string sourceDir, string currentExePath)
        {
            sourceDir = Path.GetFullPath(sourceDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            currentExePath = Path.GetFullPath(currentExePath);
            string appDir = Path.GetDirectoryName(currentExePath);
            string appDirPrefix = appDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string sourceExePath = Path.Combine(sourceDir, typeof(UpdateManager).Assembly.GetName().Name + ".exe");
            string oldExePath = currentExePath + ".old";

            if (!File.Exists(sourceExePath))
            {
                throw new FileNotFoundException(null, sourceExePath);
            }

            ValidateUpdateExecutable(sourceExePath);

            string GetDestinationPath(string file)
            {
                if (file.Equals(sourceExePath, StringComparison.OrdinalIgnoreCase))
                {
                    return currentExePath;
                }

                if (file.Equals(sourceExePath + ".config", StringComparison.OrdinalIgnoreCase))
                {
                    return currentExePath + ".config";
                }

                return Path.GetFullPath(Path.Combine(appDir, file.Substring(sourceDir.Length + 1)));
            }

            var filesToCopy = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            Array.Sort(filesToCopy, StringComparer.OrdinalIgnoreCase);
            var destinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Validate the replacement and every destination before renaming the running executable.
            foreach (var file in filesToCopy)
            {
                string relativePath = file.Substring(sourceDir.Length + 1);
                string destPath = GetDestinationPath(file);

                if (!destPath.StartsWith(appDirPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Update package contains a file that escapes the application directory: {relativePath}");
                }

                if (!destinations.Add(destPath) || destPath.Equals(oldExePath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException();
                }
            }

            string backupDirectory = Path.Combine(sourceDir, ".rollback-" + Guid.NewGuid().ToString("N"));
            var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var touchedFiles = new List<string>();
            var createdDirectories = new List<string>();
            bool executableMoved = false;
            bool retainBackups = false;
            try
            {
                Directory.CreateDirectory(backupDirectory);
                foreach (string destination in destinations)
                {
                    if (!destination.Equals(currentExePath, StringComparison.OrdinalIgnoreCase) && File.Exists(destination))
                    {
                        string backup = Path.Combine(backupDirectory, backups.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        File.Copy(destination, backup);
                        backups.Add(destination, backup);
                    }
                }

                // Windows allows renaming a running executable.
                if (File.Exists(oldExePath))
                {
                    File.Delete(oldExePath);
                }

                File.Move(currentExePath, oldExePath);
                executableMoved = true;

                foreach (var file in filesToCopy)
                {
                    string destPath = GetDestinationPath(file);
                    string destDir = Path.GetDirectoryName(destPath);

                    var missingDirectories = new Stack<string>();
                    for (string directory = destDir; !Directory.Exists(directory); directory = Path.GetDirectoryName(directory))
                    {
                        missingDirectories.Push(directory);
                    }

                    while (missingDirectories.Count > 0)
                    {
                        string directory = missingDirectories.Pop();
                        Directory.CreateDirectory(directory);
                        createdDirectories.Add(directory);
                    }

                    touchedFiles.Add(destPath);
                    File.Copy(file, destPath, overwrite: true);
                }
            }
            catch (Exception installError)
            {
                var rollbackErrors = new List<Exception>();
                for (int i = touchedFiles.Count - 1; i >= 0; i--)
                {
                    string destination = touchedFiles[i];
                    if (destination.Equals(currentExePath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        if (backups.TryGetValue(destination, out string backup))
                        {
                            File.Copy(backup, destination, overwrite: true);
                        }
                        else if (File.Exists(destination))
                        {
                            File.Delete(destination);
                        }
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        rollbackErrors.Add(ex);
                    }
                }

                if (executableMoved)
                {
                    try
                    {
                        if (File.Exists(currentExePath))
                        {
                            File.Delete(currentExePath);
                        }
                        File.Move(oldExePath, currentExePath);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        rollbackErrors.Add(ex);
                    }
                }

                for (int i = createdDirectories.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        Directory.Delete(createdDirectories[i]);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        rollbackErrors.Add(ex);
                    }
                }

                if (rollbackErrors.Count > 0)
                {
                    retainBackups = true;
                    Trace.TraceError($"Update rollback could not complete. Backups retained at {backupDirectory}");
                    rollbackErrors.Insert(0, installError);
                    throw new AggregateException(rollbackErrors);
                }

                throw;
            }
            finally
            {
                if (!retainBackups && Directory.Exists(backupDirectory))
                {
                    try
                    {
                        Directory.Delete(backupDirectory, recursive: true);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Trace.TraceWarning($"Could not remove update backups at {backupDirectory}: {ex}");
                    }
                }
            }
        }

        private static void ValidateUpdateExecutable(string path)
        {
            // Isolate reflection-only loads so retries never reuse an earlier payload's metadata.
            var domain = AppDomain.CreateDomain("Update payload validation");
            try
            {
                var validator = (ExecutableValidator)domain.CreateInstanceFromAndUnwrap(
                    typeof(UpdateManager).Assembly.Location, typeof(ExecutableValidator).FullName);
                validator.Validate(path, typeof(UpdateManager).Assembly.GetName().Name);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        public sealed class ExecutableValidator : MarshalByRefObject
        {
            public void Validate(string path, string expectedName)
            {
                var assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(path));
                assembly.ManifestModule.GetPEKind(out PortableExecutableKinds kind, out ImageFileMachine machine);
                if (assembly.GetName().Name != expectedName
                    || assembly.EntryPoint == null || !assembly.EntryPoint.IsStatic
                    || (kind & PortableExecutableKinds.ILOnly) == 0
                    || (machine != ImageFileMachine.I386 && machine != ImageFileMachine.AMD64)
                    || (machine == ImageFileMachine.AMD64 && !Environment.Is64BitOperatingSystem))
                {
                    throw new InvalidDataException();
                }

                using (var reader = new BinaryReader(File.OpenRead(path)))
                {
                    reader.BaseStream.Position = 0x3c;
                    int peOffset = reader.ReadInt32();
                    reader.BaseStream.Position = peOffset + 22;
                    if ((reader.ReadUInt16() & 0x2000) != 0)
                    {
                        throw new InvalidDataException();
                    }
                }
            }
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
