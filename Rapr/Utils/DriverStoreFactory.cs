using System;
using System.Diagnostics;

using Rapr.Properties;

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
        public static IDriverStore CreateOnlineDriverStore()
        {
            _ = Enum.TryParse(Settings.Default.DriverStoreOption, out DriverStoreOption driverStoreOption);

            switch (driverStoreOption)
            {
                case DriverStoreOption.Native:
                    return new NativeDriverStore();
                case DriverStoreOption.DISM:
                    return new DismUtil();
                case DriverStoreOption.PnpUtil:
                    return new PnpUtil();
                default:
                    throw new ArgumentException($"Unsupported driver store option: {driverStoreOption}");
            }
        }

        public static IDriverStore CreateOfflineDriverStore(string imagePath)
        {
            return new DismUtil(imagePath);
        }

        /// <summary>
        /// Migrates the legacy UseNativeDriverStore boolean setting to the new DriverStoreOption enum.
        /// </summary>
        public static void MigrateDriverStoreSettings()
        {
            // Check if we need to migrate from the old UseNativeDriverStore setting
            // We only migrate if DriverStoreOption is still at its default value (0 = Native)
            // and the old setting exists with a non-default value
            bool useNativeDriverStore = Properties.Settings.Default.UseNativeDriverStore;

            DriverStoreOption newOption = (useNativeDriverStore && DSEFormHelper.IsNativeDriverStoreSupported)
                ? DriverStoreOption.Native
                : (DSEFormHelper.IsWin8OrNewer && DismUtil.IsDismAvailable)
                    ? DriverStoreOption.DISM
                    : DriverStoreOption.PnpUtil;

            Properties.Settings.Default.DriverStoreOption = newOption.ToString();

            Trace.TraceInformation($"Migrated UseNativeDriverStore setting: {useNativeDriverStore} -> DriverStoreOption: {newOption}");
        }

        /// <summary>
        /// Validates the current driver store option from settings and automatically adjusts it 
        /// to a supported option based on system capabilities if the current option is not available.
        /// </summary>
        public static void ValidateDriverStoreOption()
        {
            // Get the current driver store option from settings
            _ = Enum.TryParse(Settings.Default.DriverStoreOption, out DriverStoreOption driverStoreOption);

            // Validate and adjust the option based on system capabilities
            if (driverStoreOption == DriverStoreOption.Native && !DSEFormHelper.IsNativeDriverStoreSupported)
            {
                // Fall back to DISM if available, otherwise PnpUtil
                if (DSEFormHelper.IsWin8OrNewer && DismUtil.IsDismAvailable)
                {
                    driverStoreOption = DriverStoreOption.DISM;
                }
                else
                {
                    driverStoreOption = DriverStoreOption.PnpUtil;
                }
            }
            else if (driverStoreOption == DriverStoreOption.DISM && !DismUtil.IsDismAvailable)
            {
                // Fall back to PnpUtil if DISM is not available
                driverStoreOption = DriverStoreOption.PnpUtil;
            }
            else if (driverStoreOption == DriverStoreOption.PnpUtil && !DSEFormHelper.IsPnpUtilSupported)
            {
                if (DSEFormHelper.IsNativeDriverStoreSupported)
                {
                    driverStoreOption = DriverStoreOption.Native;
                }
                else if (DismUtil.IsDismAvailable)
                {
                    driverStoreOption = DriverStoreOption.DISM;
                }
                else
                {
                    // This shouldn't happen, but just in case
                    driverStoreOption = DriverStoreOption.DISM;
                }
            }

            if (Settings.Default.DriverStoreOption != driverStoreOption.ToString())
            {
                Settings.Default.DriverStoreOption = driverStoreOption.ToString();
            }
        }
    }
}
