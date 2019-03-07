namespace Rapr.Utils
{
    public class DeviceDriverInfo
    {
        public string DeviceName { get; }

        public string DriverInf { get; }

        public bool? IsPresent { get; }

        public DeviceDriverInfo(string name, string inf, bool? isPresent)
        {
            this.DeviceName = name;
            this.DriverInf = inf;
            this.IsPresent = isPresent;
        }

        public override string ToString()
        {
            return $"Name: {this.DeviceName}, Inf: {this.DriverInf}, Present: {this.IsPresent}";
        }
    }
}
