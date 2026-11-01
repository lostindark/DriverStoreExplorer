using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapr.Utils
{
    /// <summary>
    /// Facts about a package that depend on the rest of the store, such as whether a newer
    /// version of the same driver is installed.
    /// </summary>
    public sealed class DriverInsights
    {
        private readonly Dictionary<DriverStoreEntry, IReadOnlyList<IGrouping<(Version Version, DateTime Date), DriverStoreEntry>>> versionsByEntry =
            new Dictionary<DriverStoreEntry, IReadOnlyList<IGrouping<(Version Version, DateTime Date), DriverStoreEntry>>>();

        /// <param name="entries">All packages in the store.</param>
        /// <param name="devicesKnown">False when the backend can't tell which devices use a package, as for offline images.</param>
        public DriverInsights(IEnumerable<DriverStoreEntry> entries, bool devicesKnown)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            this.DevicesKnown = devicesKnown;

            foreach (var versions in GroupVersions(entries))
            {
                foreach (var entry in versions.SelectMany(version => version))
                {
                    this.versionsByEntry[entry] = versions;
                }
            }
        }

        public bool DevicesKnown { get; }

        /// <summary>
        /// Groups the packages of each driver (same class, extension, provider and INF name) by
        /// version and date, newest first. Only drivers with more than one version are returned.
        /// This is the grouping "Select Old Driver(s)" uses.
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<IGrouping<(Version Version, DateTime Date), DriverStoreEntry>>> GroupVersions(IEnumerable<DriverStoreEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            return entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.DriverInfName)
                    && entry.DriverInfName != DriverStoreRepository.UnknownInfName
                    && entry.DriverInfName != "ntprint.inf")
                .GroupBy(entry => new { entry.DriverClass, entry.DriverExtensionId, entry.DriverPkgProvider, entry.DriverInfName })
                .Select(drivers => (IReadOnlyList<IGrouping<(Version Version, DateTime Date), DriverStoreEntry>>)drivers
                    .GroupBy(entry => (Version: entry.DriverVersion, Date: entry.DriverDate))
                    .OrderByDescending(g => g.Key.Version)
                    .ThenByDescending(g => g.Key.Date)
                    .ToList())
                .Where(versions => versions.Count > 1)
                .ToList();
        }

        /// <summary>
        /// The newest version of the same driver when <paramref name="entry"/> is an older one,
        /// preferring a package that a connected device uses. Null when it is already the newest.
        /// </summary>
        public DriverStoreEntry GetNewerVersion(DriverStoreEntry entry)
        {
            if (entry == null || !this.versionsByEntry.TryGetValue(entry, out var versions) || versions[0].Contains(entry))
            {
                return null;
            }

            // Several packages can share the newest version and date. Name a connected device
            // when there is one, then any device, so the note doesn't skip the one in use.
            return versions[0].FirstOrDefault(newer => !string.IsNullOrEmpty(newer.DeviceName) && newer.DevicePresent != false)
                ?? versions[0].FirstOrDefault(newer => !string.IsNullOrEmpty(newer.DeviceName))
                ?? versions[0].First();
        }

        /// <summary>
        /// An older version that no device uses: the packages "Select Old Driver(s)" picks when
        /// boot-critical drivers are included.
        /// </summary>
        public bool IsOldVersion(DriverStoreEntry entry)
        {
            if (!this.DevicesKnown || entry == null || !this.versionsByEntry.TryGetValue(entry, out var versions))
            {
                return false;
            }

            return versions
                .Skip(1)
                .Any(version => version.Contains(entry) && version.All(e => !e.HasDeviceAssociation));
        }

        public bool HasNoDevice(DriverStoreEntry entry) => this.DevicesKnown && entry != null && !entry.HasDeviceAssociation;

        public static bool IsBootCritical(DriverStoreEntry entry) => entry?.BootCritical == true;

        public static bool IsExtension(DriverStoreEntry entry) => entry != null && entry.DriverExtensionId != Guid.Empty;
    }
}
