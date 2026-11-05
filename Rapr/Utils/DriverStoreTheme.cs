using System;
using System.Diagnostics;
using System.Windows.Forms;

using BrightIdeasSoftware;

namespace Rapr.Utils
{
    internal static class DriverStoreTheme
    {
        internal static void Initialize()
        {
            string setting = Properties.Settings.Default.ThemeMode;
            if (!Enum.TryParse(setting, out ObjectListViewColorMode mode)
                || (mode != ObjectListViewColorMode.System && mode != ObjectListViewColorMode.Light && mode != ObjectListViewColorMode.Dark))
            {
                Trace.TraceWarning($"Unknown theme setting '{setting}'; following the Windows setting.");
                mode = ObjectListViewColorMode.System;
            }

            FormTheme.SetColorMode(mode);
        }

        internal static void Attach(Form form, Action<IThemeColors> themeChanged = null)
        {
            Initialize();
            FormTheme.Attach(form, prepareFirstFrame: true, themeChanged: themeChanged);
        }
    }
}
