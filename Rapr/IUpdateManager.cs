using System;
using System.Threading.Tasks;

namespace Rapr
{
    public interface IUpdateManager
    {
        Task<VersionInfo> GetLatestVersionInfo();

        Task ApplyUpdateAsync(VersionInfo versionInfo, IProgress<float> progress);
    }
}