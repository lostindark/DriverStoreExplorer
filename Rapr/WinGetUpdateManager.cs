using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Rapr
{
    public class WinGetUpdateManager : IUpdateManager, IDisposable
    {
        private const string WinGetPackageId = "lostindark.DriverStoreExplorer";
        private readonly UpdateManager updateManager = new UpdateManager();

        public bool HandlesRestart => true;

        public async Task<VersionInfo> GetLatestVersionInfo()
        {
            // Use winget search to get the latest available version — version numbers
            // in the output are always locale-independent (digits and dots).
            string output = await RunCommandAsync(
                "winget",
                $"search --id {WinGetPackageId} --exact --source winget --disable-interactivity --accept-source-agreements")
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(output))
            {
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool pastSeparator = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed.StartsWith("-", StringComparison.Ordinal))
                    {
                        pastSeparator = true;
                        continue;
                    }

                    if (pastSeparator)
                    {
                        // Extract the last whitespace-delimited token — that's the version
                        var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0 && Version.TryParse(parts[parts.Length - 1], out var version))
                        {
                            return new VersionInfo
                            {
                                Version = version,
                                PageUrl = new Uri($"https://github.com/lostindark/DriverStoreExplorer/releases/tag/v{parts[parts.Length - 1]}"),
                            };
                        }
                    }
                }
            }

            // Fall back to GitHub API if winget command fails
            return await this.updateManager.GetLatestVersionInfo().ConfigureAwait(false);
        }

        public Task ApplyUpdateAsync(VersionInfo versionInfo, IProgress<float> progress)
        {
            int pid = Process.GetCurrentProcess().Id;
            string exePath = Assembly.GetExecutingAssembly().Location;

            string scriptDir = Path.Combine(Path.GetTempPath(), "DriverStoreExplorer");
            if (!Directory.Exists(scriptDir))
            {
                Directory.CreateDirectory(scriptDir);
            }

            string scriptPath = Path.Combine(scriptDir, "winget_update.cmd");
            string script = $@"@echo off
:wait
tasklist /FI ""PID eq {pid}"" 2>NUL | find /I ""{pid}"" >NUL
if %errorlevel%==0 (timeout /t 1 /nobreak >nul & goto wait)
winget upgrade --id {WinGetPackageId} --silent --disable-interactivity --accept-source-agreements --accept-package-agreements
start """" ""{exePath}""
del ""%~f0""
";

            File.WriteAllText(scriptPath, script);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.updateManager.Dispose();
        }

        private static Task<string> RunCommandAsync(string fileName, string arguments)
        {
            return Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output;
                    }
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
