using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

using Bluegrams.Application;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rapr.Tests
{
    [TestClass]
    public class SettingsMigrationTests
    {
        private static readonly Func<ApplicationSettingsBase, string, string, Action<bool>, bool> UsePortableSettings =
            (Func<ApplicationSettingsBase, string, string, Action<bool>, bool>)typeof(DriverStoreType).Assembly
                .GetType("Rapr.Utils.SettingsMigration")
                .GetMethod("UsePortableSettings", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<ApplicationSettingsBase, string, string, Action<bool>, bool>));

        private string directory;
        private string previousDirectory;
        private string previousFileName;

        [TestInitialize]
        public void Initialize()
        {
            this.directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "RaprSettingsTests-" + Guid.NewGuid().ToString("N"))).FullName;
            this.previousDirectory = PortableSettingsProvider.SettingsDirectory;
            this.previousFileName = PortableSettingsProvider.SettingsFileName;
        }

        [TestCleanup]
        public void Cleanup()
        {
            PortableSettingsProvider.SettingsDirectory = this.previousDirectory;
            PortableSettingsProvider.SettingsFileName = this.previousFileName;
            Directory.Delete(this.directory, recursive: true);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("Native")]
        [DataRow("PnpUtil")]
        public void BackendPresenceMatchesProductionProviderPersistedSettings(string backend)
        {
            string path = Path.Combine(this.directory, "user.config");
            var context = new SettingsContext { ["GroupName"] = "MigrationTests", ["SettingsKey"] = "instance" };
            var property = new SettingsProperty("DriverStoreOption")
            {
                PropertyType = typeof(string),
                DefaultValue = "Native",
                SerializeAs = SettingsSerializeAs.String
            };
            property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());
            var document = new XmlDocument();
            document.LoadXml("<configuration><configSections><sectionGroup name=\"userSettings\" type=\"System.Configuration.UserSettingsGroup, System\">"
                + "<section name=\"MigrationTests.instance\" type=\"System.Configuration.ClientSettingsSection, System\" allowExeDefinition=\"MachineToLocalUser\" />"
                + "</sectionGroup></configSections><userSettings><MigrationTests.instance /></userSettings></configuration>");
            ((XmlElement)document.SelectSingleNode("//sectionGroup")).SetAttribute("type", typeof(UserSettingsGroup).AssemblyQualifiedName);
            ((XmlElement)document.SelectSingleNode("//section")).SetAttribute("type", typeof(ClientSettingsSection).AssemblyQualifiedName);
            if (backend != null)
            {
                var element = document.CreateElement("setting");
                element.SetAttribute("name", property.Name);
                element.SetAttribute("serializeAs", "String");
                var value = document.CreateElement("value");
                value.InnerText = backend;
                element.AppendChild(value);
                document.SelectSingleNode("//MigrationTests.instance").AppendChild(element);
            }
            document.Save(path);

            var provider = new LocalFileSettingsProvider();
            provider.Initialize(null, new NameValueCollection());
            typeof(LocalFileSettingsProvider).GetField("_prevLocalConfigFileName",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(provider, path);
            var previous = provider.GetPreviousVersion(context, property);

            var hasStored = (Func<SettingsContext, string, string[], bool>)typeof(DriverStoreType).Assembly
                .GetType("Rapr.Utils.SettingsMigration")
                .GetMethod("HasStoredUserSettingInFiles", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<SettingsContext, string, string[], bool>));

            Assert.AreEqual(previous != null, hasStored(context, property.Name, new[] { path }));
            Assert.AreEqual(backend != null, hasStored(context, property.Name, new[] { path }));
            if (backend != null)
            {
                Assert.AreEqual(backend, previous.PropertyValue);
            }
        }

        [TestMethod]
        public void ExistingPortableSettingsDoNotReadTheUnusedSourceProvider()
        {
            UsePortableSettings(CreateSettings(false), this.directory, "portable.config", hasOption => { });
            var settings = new TestSettings();
            var provider = (MemorySettingsProvider)settings.Providers["MemorySettingsProvider"];
            provider.FailReads = true;

            Assert.IsFalse(UsePortableSettings(settings, this.directory, "portable.config", hasOption => Assert.Fail()));

            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.AreEqual("PnpUtil", settings.DriverStoreOption);
        }

        [TestMethod]
        public void GeneratedApplicationSettingsRetainTheirTypedValuesAfterImport()
        {
            var settings = (ApplicationSettingsBase)Activator.CreateInstance(
                typeof(DriverStoreType).Assembly.GetType("Rapr.Properties.Settings"), nonPublic: true);
            var provider = new MemorySettingsProvider();
            provider.Initialize(null, new NameValueCollection());
            provider.Values["Language"] = CultureInfo.GetCultureInfo("fr-FR");
            provider.Values["WindowSize"] = new Size(900, 600);
            provider.Values["WindowLocation"] = new Point(25, 35);
            provider.Values["DriverStoreViewState"] = "AQIDBA==";
            provider.Values["DriverStoreOption"] = "PnpUtil";
            provider.Values["UpgradeRequired"] = false;
            settings.Providers.Clear();
            settings.Providers.Add(provider);
            foreach (SettingsProperty property in settings.Properties)
            {
                property.Provider = provider;
            }
            settings.Reload();

            UsePortableSettings(settings, this.directory, "portable.config", hasOption => Assert.IsTrue(hasOption));

            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings["Language"]);
            Assert.AreEqual(new Size(900, 600), settings["WindowSize"]);
            Assert.AreEqual(new Point(25, 35), settings["WindowLocation"]);
            Assert.AreEqual("AQIDBA==", settings["DriverStoreViewState"]);
            Assert.AreEqual("PnpUtil", settings["DriverStoreOption"]);
            Assert.AreEqual(false, settings["UpgradeRequired"]);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FirstPortableConfigurationImportsCurrentOrPreviousUserPreferences(bool upgradeRequired)
        {
            var settings = CreateSettings(upgradeRequired);
            bool migrated = UsePortableSettings(settings, this.directory, "portable.config", hasOption => Assert.IsTrue(hasOption));

            Assert.IsTrue(migrated);
            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.AreEqual(new Point(25, 35), settings.WindowLocation);
            Assert.AreEqual(TimeSpan.FromDays(7), settings.LogRetentionPeriod);
            Assert.AreEqual("PnpUtil", settings.DriverStoreOption);
            Assert.IsFalse(settings.UpgradeRequired);
            Assert.IsTrue(File.Exists(Path.Combine(this.directory, "portable.config")));
        }

        [TestMethod]
        public void ExistingPortablePreferencesAreNotReplacedByPerUserValues()
        {
            var first = CreateSettings(false);
            UsePortableSettings(first, this.directory, "portable.config", hasOption => { });

            var settings = CreateSettings(false);
            settings.Language = CultureInfo.GetCultureInfo("de-DE");
            settings.DriverStoreOption = "Native";

            Assert.IsFalse(UsePortableSettings(settings, this.directory, "portable.config", hasOption => Assert.Fail()));

            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.AreEqual("PnpUtil", settings.DriverStoreOption);
        }

        [TestMethod]
        public void FailedFirstSaveRestoresTheSourceProviderAndKeepsMigrationPending()
        {
            var settings = CreateSettings(true);
            var sourceProvider = settings.Providers["MemorySettingsProvider"];
            settings.SettingsSaving += (sender, e) => e.Cancel = true;

            Assert.ThrowsExactly<FileNotFoundException>(() =>
                UsePortableSettings(settings, this.directory, "portable.config", hasOption => { }));

            Assert.AreSame(sourceProvider, settings.Providers["MemorySettingsProvider"]);
            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.AreEqual("PnpUtil", settings.DriverStoreOption);
            Assert.IsTrue(settings.UpgradeRequired);
            Assert.IsFalse(File.Exists(Path.Combine(this.directory, "portable.config")));
        }

        [TestMethod]
        public void FailedExistingSavePreservesThePortableFileAndPreferences()
        {
            var settings = CreateSettings(false);
            UsePortableSettings(settings, this.directory, "portable.config", hasOption => { });
            settings.UpgradeRequired = true;
            settings.Save();
            string path = Path.Combine(this.directory, "portable.config");
            byte[] original = File.ReadAllBytes(path);
            settings.SettingsSaving += (sender, e) =>
            {
                File.WriteAllText(path, "<unexpected />");
                e.Cancel = true;
            };

            Assert.ThrowsExactly<InvalidDataException>(() =>
                UsePortableSettings(settings, this.directory, "portable.config", hasOption => { }));

            CollectionAssert.AreEqual(original, File.ReadAllBytes(Path.Combine(this.directory, "portable.config")));
            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.IsTrue(settings.UpgradeRequired);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MissingLegacyBackendSettingCanBeMigratedWithoutOverwritingExistingChoices(bool upgradeRequired)
        {
            var settings = new TestSettings();
            settings.UpgradeRequired = upgradeRequired;
            UsePortableSettings(settings, this.directory, "portable.config", hasOption =>
            {
                Assert.IsFalse(hasOption);
                settings.DriverStoreOption = "DISM";
            });

            Assert.AreEqual("DISM", settings.DriverStoreOption);
            Assert.IsFalse(settings.UpgradeRequired);
        }

        [TestMethod]
        public void ExistingLegacyPortableConfigurationReceivesTheMissingBackendSetting()
        {
            var first = CreateSettings(false);
            UsePortableSettings(first, this.directory, "portable.config", hasOption => { });
            string path = Path.Combine(this.directory, "portable.config");
            var document = new XmlDocument();
            document.Load(path);
            var option = document.SelectSingleNode("//DriverStoreOption");
            option.ParentNode.RemoveChild(option);
            document.Save(path);

            var settings = CreateSettings(false);
            UsePortableSettings(settings, this.directory, "portable.config", hasOption =>
            {
                Assert.IsFalse(hasOption);
                settings.DriverStoreOption = "DISM";
            });

            Assert.AreEqual("DISM", settings.DriverStoreOption);
            Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), settings.Language);
            Assert.IsFalse(settings.UpgradeRequired);
        }

        private static TestSettings CreateSettings(bool upgradeRequired)
        {
            var settings = new TestSettings();
            var provider = (MemorySettingsProvider)settings.Providers["MemorySettingsProvider"];
            var values = upgradeRequired ? provider.PreviousValues : provider.Values;
            values["Language"] = CultureInfo.GetCultureInfo("fr-FR");
            values["WindowLocation"] = new Point(25, 35);
            values["LogRetentionPeriod"] = TimeSpan.FromDays(7);
            values["DriverStoreOption"] = "PnpUtil";
            provider.Values["UpgradeRequired"] = upgradeRequired;
            return settings;
        }

        [SettingsProvider(typeof(MemorySettingsProvider))]
        public sealed class TestSettings : ApplicationSettingsBase
        {
            [UserScopedSetting, DefaultSettingValue("(Default)"), SettingsManageability(SettingsManageability.Roaming)]
            public CultureInfo Language
            {
                get => (CultureInfo)this[nameof(this.Language)];
                set => this[nameof(this.Language)] = value;
            }

            [UserScopedSetting, DefaultSettingValue("0, 0")]
            public Point WindowLocation
            {
                get => (Point)this[nameof(this.WindowLocation)];
                set => this[nameof(this.WindowLocation)] = value;
            }

            [UserScopedSetting, DefaultSettingValue("180.00:00:00")]
            public TimeSpan LogRetentionPeriod
            {
                get => (TimeSpan)this[nameof(this.LogRetentionPeriod)];
                set => this[nameof(this.LogRetentionPeriod)] = value;
            }

            [UserScopedSetting, DefaultSettingValue("Native")]
            public string DriverStoreOption
            {
                get => (string)this[nameof(this.DriverStoreOption)];
                set => this[nameof(this.DriverStoreOption)] = value;
            }

            [UserScopedSetting, DefaultSettingValue("True")]
            public bool UpgradeRequired
            {
                get => (bool)this[nameof(this.UpgradeRequired)];
                set => this[nameof(this.UpgradeRequired)] = value;
            }
        }

        public sealed class MemorySettingsProvider : SettingsProvider, IApplicationSettingsProvider
        {
            public bool FailReads { get; set; }
            public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> PreviousValues { get; } = new Dictionary<string, object>();
            public override string ApplicationName { get; set; } = "Tests";

            public override void Initialize(string name, NameValueCollection config)
            {
                base.Initialize(nameof(MemorySettingsProvider), config);
            }

            public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
            {
                if (this.FailReads)
                {
                    throw new ConfigurationErrorsException("Source configuration cannot be read.");
                }

                var result = new SettingsPropertyValueCollection();
                foreach (SettingsProperty property in properties)
                {
                    var value = new SettingsPropertyValue(property);
                    if (this.Values.TryGetValue(property.Name, out object stored))
                    {
                        value.PropertyValue = stored;
                    }

                    value.IsDirty = false;
                    result.Add(value);
                }

                return result;
            }

            public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
            {
                foreach (SettingsPropertyValue value in values)
                {
                    this.Values[value.Name] = value.PropertyValue;
                }
            }

            public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
            {
                return new SettingsPropertyValue(property);
            }

            public void Reset(SettingsContext context)
            {
                this.Values.Clear();
            }

            public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
            {
                foreach (var value in this.PreviousValues)
                {
                    this.Values[value.Key] = value.Value;
                }
            }
        }
    }
}
