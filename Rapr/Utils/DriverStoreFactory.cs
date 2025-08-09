using System;

namespace Rapr.Utils
{
    public enum DriverStoreOption
    {
        Native,
        DISM,
        PnpUtil
    }

    public static class DriverStoreFactory
    {
        public static IDriverStore CreateOnlineDriverStore(DriverStoreOption option)
        {
            switch (option)
            {
                case DriverStoreOption.Native:
                    return new NativeDriverStore();
                case DriverStoreOption.DISM:
                    return new DismUtil();
                case DriverStoreOption.PnpUtil:
                    return new PnpUtil();
                default:
                    throw new ArgumentException($"Unsupported driver store option: {option}");
            }
        }

        public static IDriverStore CreateOfflineDriverStore(string imagePath)
        {
            return new DismUtil(imagePath);
        }
    }
}
