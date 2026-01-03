using System;

namespace Rapr.Utils
{
    public class DeviceDriverInfo
    {
        public string DeviceId { get; }

        public string DeviceName { get; }

        public string DriverInf { get; }

        public DateTime DriverDate { get; }

        public Version DriverVersion { get; }

        public bool? IsPresent { get; }

        public string[] ExtendedInfs { get; set; }

        public DeviceDriverInfo(string deviceId, string name, string inf, DateTime driverDate, Version driverVersion, bool? isPresent, string[] extendedInfs)
        {
            this.DeviceId = deviceId;
            this.DeviceName = name;
            this.DriverInf = inf;
            this.DriverDate = driverDate;
            this.DriverVersion = driverVersion;
            this.IsPresent = isPresent;
            this.ExtendedInfs = extendedInfs;
        }

        public override string ToString()
        {
            return $"Id: {this.DeviceId}, Name: {this.DeviceName}, Inf: {this.DriverInf}, DriverDate: {this.DriverDate}, DriverVersion: {this.DriverVersion}, Present: {this.IsPresent}, ExtendedInfs: {string.Join(", ", this.ExtendedInfs)}";
        }
    }
}
