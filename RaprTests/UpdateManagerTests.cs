using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rapr.Tests
{
    [TestClass]
    public class UpdateManagerTests
    {
        private static readonly Action<string, string> InstallUpdate =
            (Action<string, string>)typeof(UpdateManager)
                .GetMethod("InstallUpdate", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Action<string, string>));

        private static readonly Func<string, VersionInfo> ParseLatestRelease =
            (Func<string, VersionInfo>)typeof(UpdateManager)
                .GetMethod("ParseLatestRelease", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<string, VersionInfo>));

        private string testDirectory;
        private string sourceDirectory;
        private string appDirectory;

        [TestInitialize]
        public void Initialize()
        {
            this.testDirectory = Path.Combine(Path.GetTempPath(), "RaprUpdateTests-" + Guid.NewGuid().ToString("N"));
            this.sourceDirectory = Directory.CreateDirectory(Path.Combine(this.testDirectory, "source")).FullName;
            this.appDirectory = Directory.CreateDirectory(Path.Combine(this.testDirectory, "app")).FullName;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(this.testDirectory, recursive: true);
        }

        [TestMethod]
        public void LatestReleaseIsReadFromTheGitHubResponse()
        {
            const string Json = @"{
                ""tag_name"": ""v1.0.63"",
                ""html_url"": ""https://github.com/lostindark/DriverStoreExplorer/releases/tag/v1.0.63"",
                ""draft"": false,
                ""assets"": [
                    {
                        ""name"": ""DriverStoreExplorer-v1.0.63.zip"",
                        ""size"": 1058111,
                        ""digest"": ""sha256:2c26b46b68ffc68ff99b453c1d30413413422d706483bfa0f98a5e886266e7ae"",
                        ""browser_download_url"": ""https://github.com/lostindark/DriverStoreExplorer/releases/download/v1.0.63/DriverStoreExplorer-v1.0.63.zip""
                    }
                ]
            }";

            VersionInfo info = ParseLatestRelease(Json);

            Assert.AreEqual(new Version(1, 0, 63), info.Version);
            Assert.AreEqual("https://github.com/lostindark/DriverStoreExplorer/releases/tag/v1.0.63", info.PageUrl.ToString());
            Assert.AreEqual("https://github.com/lostindark/DriverStoreExplorer/releases/download/v1.0.63/DriverStoreExplorer-v1.0.63.zip", info.DownloadUrl.ToString());
            Assert.AreEqual("2c26b46b68ffc68ff99b453c1d30413413422d706483bfa0f98a5e886266e7ae", info.Sha256);
        }

        [TestMethod]
        public void TheAppZipIsPickedEvenIfItIsNotTheFirstAsset()
        {
            const string Json = @"{
                ""tag_name"": ""v1.0.64"",
                ""html_url"": ""https://github.com/lostindark/DriverStoreExplorer/releases/tag/v1.0.64"",
                ""assets"": [
                    {
                        ""name"": ""checksums.txt"",
                        ""browser_download_url"": ""https://github.com/lostindark/DriverStoreExplorer/releases/download/v1.0.64/checksums.txt""
                    },
                    {
                        ""name"": ""DriverStoreExplorer-v1.0.64.zip"",
                        ""browser_download_url"": ""https://github.com/lostindark/DriverStoreExplorer/releases/download/v1.0.64/DriverStoreExplorer-v1.0.64.zip""
                    }
                ]
            }";

            Assert.EndsWith("/DriverStoreExplorer-v1.0.64.zip", ParseLatestRelease(Json).DownloadUrl.ToString());
        }

        [TestMethod]
        public void ReleaseWithoutAssetsHasNoUpdate()
        {
            Assert.IsNull(ParseLatestRelease(@"{ ""tag_name"": ""v1.0.63"", ""html_url"": ""https://github.com/"", ""assets"": [] }"));
        }

        [TestMethod]
        public void SameNameLibraryCannotReplaceTheExecutable()
        {
            string currentExePath = Path.Combine(this.appDirectory, "Rapr.exe");
            File.WriteAllText(currentExePath, "original executable");
            var library = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("Rapr"), AssemblyBuilderAccess.Save, this.sourceDirectory);
            library.DefineDynamicModule("Library", "Rapr.dll");
            library.Save("Rapr.dll");
            File.Move(Path.Combine(this.sourceDirectory, "Rapr.dll"), Path.Combine(this.sourceDirectory, "Rapr.exe"));

            Assert.ThrowsExactly<InvalidDataException>(() => InstallUpdate(this.sourceDirectory, currentExePath));

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.IsFalse(File.Exists(currentExePath + ".old"));
        }

        [TestMethod]
        [DataRow("Rapr.exe")]
        [DataRow("RenamedDriverStoreExplorer.exe")]
        public void UpdateReplacesTheCurrentExecutableAndItsConfiguration(string fileName)
        {
            string currentExePath = Path.Combine(this.appDirectory, fileName);
            File.WriteAllText(currentExePath, "original executable");
            File.WriteAllText(currentExePath + ".config", "original configuration");
            this.CreateExecutablePayload();
            File.WriteAllText(Path.Combine(this.sourceDirectory, "Rapr.exe.config"), "updated configuration");

            InstallUpdate(this.sourceDirectory, currentExePath);

            CollectionAssert.AreEqual(
                File.ReadAllBytes(typeof(UpdateManager).Assembly.Location),
                File.ReadAllBytes(currentExePath));
            Assert.AreEqual("updated configuration", File.ReadAllText(currentExePath + ".config"));
            Assert.AreEqual("original executable", File.ReadAllText(currentExePath + ".old"));
            Assert.AreEqual(3, Directory.GetFiles(this.appDirectory).Length);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MissingOrInvalidExecutableDoesNotModifyTheInstallation(bool invalidExecutable)
        {
            string currentExePath = Path.Combine(this.appDirectory, "Rapr.exe");
            File.WriteAllText(currentExePath, "original executable");
            File.WriteAllText(currentExePath + ".old", "previous backup");
            File.WriteAllText(Path.Combine(this.sourceDirectory, "README.txt"), "not an executable");

            if (invalidExecutable)
            {
                File.WriteAllText(Path.Combine(this.sourceDirectory, "Rapr.exe"), "invalid executable");
                Assert.ThrowsExactly<BadImageFormatException>(() => InstallUpdate(this.sourceDirectory, currentExePath));
            }
            else
            {
                Assert.ThrowsExactly<FileNotFoundException>(() => InstallUpdate(this.sourceDirectory, currentExePath));
            }

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.AreEqual("previous backup", File.ReadAllText(currentExePath + ".old"));
            Assert.IsFalse(File.Exists(Path.Combine(this.appDirectory, "README.txt")));
        }

        [TestMethod]
        [DataRow("Renamed.exe")]
        [DataRow("Renamed.exe.old")]
        [DataRow("Renamed.exe.config")]
        public void ConflictingDestinationIsRejectedBeforeInstallation(string conflictingFile)
        {
            string currentExePath = Path.Combine(this.appDirectory, "Renamed.exe");
            File.WriteAllText(currentExePath, "original executable");
            this.CreateExecutablePayload();
            File.WriteAllText(Path.Combine(this.sourceDirectory, "Rapr.exe.config"), "updated configuration");
            File.WriteAllText(Path.Combine(this.sourceDirectory, conflictingFile), "conflicting file");

            Assert.ThrowsExactly<InvalidDataException>(() => InstallUpdate(this.sourceDirectory, currentExePath));

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.IsFalse(File.Exists(currentExePath + ".old"));
        }

        [TestMethod]
        public void UnexpectedAssemblyIsRejectedBeforeInstallation()
        {
            string currentExePath = Path.Combine(this.appDirectory, "Rapr.exe");
            File.WriteAllText(currentExePath, "original executable");
            File.Copy(typeof(UpdateManagerTests).Assembly.Location, Path.Combine(this.sourceDirectory, "Rapr.exe"));

            Assert.ThrowsExactly<InvalidDataException>(() => InstallUpdate(this.sourceDirectory, currentExePath));

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.IsFalse(File.Exists(currentExePath + ".old"));
        }

        [TestMethod]
        public void CopyFailureRestoresTheOriginalExecutable()
        {
            string currentExePath = Path.Combine(this.appDirectory, "Renamed.exe");
            File.WriteAllText(currentExePath, "original executable");
            File.WriteAllText(Path.Combine(this.appDirectory, "blocked"), "existing file");
            this.CreateExecutablePayload();
            string blockedDirectory = Directory.CreateDirectory(Path.Combine(this.sourceDirectory, "blocked")).FullName;
            File.WriteAllText(Path.Combine(blockedDirectory, "child.txt"), "cannot copy here");

            Assert.ThrowsExactly<IOException>(() => InstallUpdate(this.sourceDirectory, currentExePath));

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.IsFalse(File.Exists(currentExePath + ".old"));
            Assert.AreEqual("existing file", File.ReadAllText(Path.Combine(this.appDirectory, "blocked")));
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void FailedUpdateRestoresConfigurationAndRemovesNewSupportingFiles(bool hasConfiguration)
        {
            string currentExePath = Path.Combine(this.appDirectory, "Renamed.exe");
            File.WriteAllText(currentExePath, "original executable");
            if (hasConfiguration)
            {
                File.WriteAllText(currentExePath + ".config", "original configuration");
            }
            this.CreateExecutablePayload();
            File.WriteAllText(Path.Combine(this.sourceDirectory, "Rapr.exe.config"), "new configuration");
            File.WriteAllText(Path.Combine(this.appDirectory, "existing.txt"), "original supporting file");
            File.WriteAllText(Path.Combine(this.sourceDirectory, "existing.txt"), "new supporting file");
            File.WriteAllText(Path.Combine(this.sourceDirectory, "new.txt"), "new file");
            string nested = Directory.CreateDirectory(Path.Combine(this.sourceDirectory, "nested")).FullName;
            File.WriteAllText(Path.Combine(nested, "new.txt"), "new nested file");
            string blocked = Directory.CreateDirectory(Path.Combine(this.sourceDirectory, "zzzblocked")).FullName;
            File.WriteAllText(Path.Combine(blocked, "failure.txt"), "cannot copy here");
            File.WriteAllText(Path.Combine(this.appDirectory, "zzzblocked"), "blocking file");

            Assert.ThrowsExactly<IOException>(() => InstallUpdate(this.sourceDirectory, currentExePath));

            Assert.AreEqual("original executable", File.ReadAllText(currentExePath));
            Assert.AreEqual(hasConfiguration, File.Exists(currentExePath + ".config"));
            if (hasConfiguration)
            {
                Assert.AreEqual("original configuration", File.ReadAllText(currentExePath + ".config"));
            }
            Assert.AreEqual("original supporting file", File.ReadAllText(Path.Combine(this.appDirectory, "existing.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(this.appDirectory, "new.txt")));
            Assert.IsFalse(Directory.Exists(Path.Combine(this.appDirectory, "nested")));
            Assert.IsFalse(File.Exists(currentExePath + ".old"));
        }

        private void CreateExecutablePayload()
        {
            File.Copy(typeof(UpdateManager).Assembly.Location, Path.Combine(this.sourceDirectory, "Rapr.exe"));
        }
    }
}
