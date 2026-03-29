using System;

namespace Rapr
{
    public class VersionInfo
    {
        public Version Version { get; set; }
        public Uri PageUrl { get; set; }
        public Uri DownloadUrl { get; set; }
        public string Sha256 { get; set; }
    }
}
