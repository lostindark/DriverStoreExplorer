using System;

namespace Rapr.Utils
{
    public static class DriverStoreFactory
    {
        private static readonly Version Win8Version = new Version(6, 2);

        public static IDriverStore CreateOnlineDriverStore()
        {
            if (IsWin8OrNewer && DismUtil.IsDismAvailable)
            {
                return new DismUtil();
            }
            else
            {
                return new PnpUtil();
            }
        }

        public static IDriverStore CreateOfflineDriverStore(string imagePath)
        {
            return new DismUtil(imagePath);
        }

        private static bool IsWin8OrNewer
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;

                return os.Platform == PlatformID.Win32NT && os.Version >= Win8Version;
            }
        }
    }
}
