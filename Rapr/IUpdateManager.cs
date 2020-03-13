using System.Threading.Tasks;

namespace Rapr
{
    public interface IUpdateManager
    {
        Task<VersionInfo> GetLatestVersionInfo();
    }
}