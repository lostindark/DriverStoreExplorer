using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;

using Bluegrams.Application;

namespace Rapr.Utils
{
    internal static class SettingsMigration
    {
        internal static bool UsePortableSettings(ApplicationSettingsBase settings, string directory, string fileName, Action<bool> migrate)
        {
            string path = Path.Combine(directory, fileName);
            byte[] originalFile = File.Exists(path) ? File.ReadAllBytes(path) : null;
            bool hasPortableSettings = originalFile?.Length > 0;
            XmlDocument document = hasPortableSettings ? LoadSettingsFile(path) : null;

            using (var probe = new FileStream(path, originalFile == null ? FileMode.CreateNew : FileMode.Open,
                FileAccess.ReadWrite, FileShare.None, 4096, originalFile == null ? FileOptions.DeleteOnClose : FileOptions.None))
            {
            }

            var providers = settings.Providers.Cast<SettingsProvider>().ToArray();
            var propertyProviders = settings.Properties.Cast<SettingsProperty>().ToDictionary(p => p.Name, p => p.Provider);
            var rollbackValues = CaptureValues(settings);
            string previousDirectory = PortableSettingsProvider.SettingsDirectory;
            string previousFileName = PortableSettingsProvider.SettingsFileName;
            bool pendingUpgrade = !hasPortableSettings && (bool)settings["UpgradeRequired"];

            try
            {
                if (pendingUpgrade)
                {
                    settings.Upgrade();
                    rollbackValues = CaptureValues(settings);
                }

                bool hasDriverStoreOption;
                if (hasPortableSettings)
                {
                    var property = settings.Properties["DriverStoreOption"];
                    bool roaming = PortableSettingsProvider.AllRoaming
                        || property.Attributes.Contains(typeof(SettingsManageabilityAttribute));
                    string scope = roaming ? "Roaming" : "PC_" + Environment.MachineName;
                    string group = XmlConvert.EncodeLocalName((string)settings.Context["GroupName"]);
                    hasDriverStoreOption = document.SelectSingleNode($"configuration/userSettings/{scope}/{group}/DriverStoreOption") != null;
                }
                else
                {
                    _ = settings["DriverStoreOption"];
                    hasDriverStoreOption = !settings.PropertyValues["DriverStoreOption"].UsingDefaultValue;
                }

                PortableSettingsProvider.SettingsDirectory = directory;
                PortableSettingsProvider.SettingsFileName = fileName;
                PortableSettingsProvider.ApplyProvider(settings);

                if (hasPortableSettings)
                {
                    rollbackValues = CaptureValues(settings);
                }
                else
                {
                    RestoreValues(settings, rollbackValues);
                }

                bool upgradeRequired = pendingUpgrade || (bool)settings["UpgradeRequired"] || !hasDriverStoreOption;
                if (!hasPortableSettings || upgradeRequired)
                {
                    if (upgradeRequired)
                    {
                        migrate(hasDriverStoreOption);
                    }

                    settings["UpgradeRequired"] = false;
                    SaveAndVerify(settings, path);
                    return true;
                }

                return false;
            }
            catch
            {
                settings.Providers.Clear();
                foreach (var provider in providers)
                {
                    settings.Providers.Add(provider);
                }

                foreach (SettingsProperty property in settings.Properties)
                {
                    property.Provider = propertyProviders[property.Name];
                }

                PortableSettingsProvider.SettingsDirectory = previousDirectory;
                PortableSettingsProvider.SettingsFileName = previousFileName;

                try
                {
                    if (originalFile != null)
                    {
                        File.WriteAllBytes(path, originalFile);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                finally
                {
                    settings.Reload();
                    RestoreValues(settings, rollbackValues);
                    if (pendingUpgrade)
                    {
                        settings["UpgradeRequired"] = true;
                    }
                }

                throw;
            }
        }

        internal static void SaveAndVerify(ApplicationSettingsBase settings, string path = null)
        {
            var expected = CaptureValues(settings);
            settings.Save();
            if (path != null)
            {
                LoadSettingsFile(path);
            }

            settings.Reload();
            foreach (var value in expected)
            {
                if (!Equals(value.Value, settings[value.Key]))
                {
                    throw new InvalidDataException();
                }
            }
        }

        private static Dictionary<string, object> CaptureValues(ApplicationSettingsBase settings)
        {
            return settings.Properties.Cast<SettingsProperty>()
                .Where(p => p.Attributes.Contains(typeof(UserScopedSettingAttribute)))
                .ToDictionary(p => p.Name, p => settings[p.Name]);
        }

        private static void RestoreValues(ApplicationSettingsBase settings, Dictionary<string, object> values)
        {
            foreach (var value in values)
            {
                settings[value.Key] = value.Value;
            }
        }

        private static XmlDocument LoadSettingsFile(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            if (document.DocumentElement?.Name != "configuration"
                || document.DocumentElement.SelectSingleNode("userSettings") == null)
            {
                throw new InvalidDataException();
            }

            return document;
        }
    }
}
