using System;

namespace Rapr.Utils
{
    public class DeviceDriverInfo
    {
        public string DeviceName { get; }

        public string DriverInf { get; }

        public DateTime DriverDate { get; }

        public Version DriverVersion { get; }

        public bool? IsPresent { get; }

        public DeviceDriverInfo(string name, string inf, DateTime driverDate, Version driverVersion, bool? isPresent)
        {
            this.DeviceName = name;
            this.DriverInf = inf;
            this.DriverDate = driverDate;
            this.DriverVersion = driverVersion;
            this.IsPresent = isPresent;
        }

        public override string ToString()
        {
            return $"Name: {this.DeviceName}, Inf: {this.DriverInf}, DriverDate: {this.DriverDate}, DriverVersion: {this.DriverVersion}, Present: {this.IsPresent}";
        }
    }
}
