using System;
using System.Linq;

namespace Rapr.Utils
{
    /// <summary>
    /// A device that is installed but not connected (a non-present devnode).
    /// </summary>
    public class GhostDevice
    {
        // Devices created by software rather than detected on a bus. Some are hidden on purpose
        // and still needed (virtual adapters, app-created devices), so they are never pre-selected.
        private static readonly string[] SoftwareEnumeratorPrefixes = { @"ROOT\", @"SWD\", @"HTREE\", @"SW\", @"UMB\", "{" };

        public string InstanceId { get; }

        public string Name { get; }

        public string ClassName { get; }

        /// <summary>
        /// When the device was last connected or disconnected (UTC), or null if Windows has no record.
        /// </summary>
        public DateTime? LastUsed { get; }

        public bool SelectedByDefault => IsSelectedByDefault(this.InstanceId);

        public GhostDevice(string instanceId, string name, string className, DateTime? lastUsed = null)
        {
            this.InstanceId = instanceId;
            this.Name = name;
            this.ClassName = className;
            this.LastUsed = lastUsed;
        }

        /// <summary>
        /// The later of the last arrival and last removal dates; either one may be missing.
        /// </summary>
        public static DateTime? LatestOf(DateTime? lastArrival, DateTime? lastRemoval) =>
            lastArrival > lastRemoval || lastRemoval == null ? lastArrival : lastRemoval;

        /// <summary>
        /// A device with no recorded use is never treated as recent, so it stays listed.
        /// </summary>
        public static bool IsUsedWithin(DateTime? lastUsed, DateTime nowUtc, int days) =>
            lastUsed.HasValue && lastUsed.Value > nowUtc.AddDays(-days);

        /// <summary>
        /// Only a devnode known to be absent is a ghost; an unknown status is treated as present.
        /// </summary>
        public static bool IsGhost(bool? isPresent) => isPresent == false;

        public static bool IsSelectedByDefault(string instanceId) =>
            !string.IsNullOrEmpty(instanceId)
            && !SoftwareEnumeratorPrefixes.Any(prefix => instanceId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        public override string ToString() => $"{this.Name} ({this.ClassName}) - {this.InstanceId}";
    }
}
