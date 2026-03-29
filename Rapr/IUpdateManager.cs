using System;
using System.Threading.Tasks;

namespace Rapr
{
    public interface IUpdateManager
    {
        bool HandlesRestart { get; }

        Task<VersionInfo> GetLatestVersionInfo();

        Task ApplyUpdateAsync(VersionInfo versionInfo, IProgress<float> progress);
    }
}