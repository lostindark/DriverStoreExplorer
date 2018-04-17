using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Rapr.Utils
{
    public class DriverStoreContent
    {
        public string InfName;
        public string Content;
        public long EstimateSize;
    }

    public class DriverStoreRepository
    {
        private static readonly Regex DriverStoreRepositoryDirNameRegex = new Regex(@"^(.+\.inf)_.+$", RegexOptions.Compiled);
        private static readonly string SystemRoot = Environment.ExpandEnvironmentVariables("%SystemRoot%");
        private static readonly string SystemRootInf = Path.Combine(SystemRoot, "INF");
        private static readonly string DriverStoreFileRepository = Path.Combine(SystemRoot, @"system32\DriverStore\FileRepository");

        private readonly DirectoryInfo driverStoreFileRepository = new DirectoryInfo(DriverStoreFileRepository);
        private readonly List<DriverStoreContent> driverStoreContents = new List<DriverStoreContent>();
        private readonly IEnumerator<DirectoryInfo> directoryInfoEnumerator;

        public DriverStoreRepository()
        {
            this.directoryInfoEnumerator = this.driverStoreFileRepository.EnumerateDirectories().GetEnumerator();
        }

        public void FindInfInfo(string infName, out string originalInfName, out long estimateSize)
        {
            originalInfName = null;
            estimateSize = -1;

            string content = GetSystemRootInfContent(infName);

            if (!string.IsNullOrEmpty(content))
            {
                DriverStoreContent driverStoreContent = FindInfInfo(content);

                if (driverStoreContent != null)
                {
                    originalInfName = driverStoreContent.InfName;
                    estimateSize = driverStoreContent.EstimateSize;
                }
            }
        }

        private DriverStoreContent FindInfInfo(string content)
        {
            for (int i = 0; i < driverStoreContents.Count; i++)
            {
                if (driverStoreContents[i].Content == content)
                {
                    DriverStoreContent driverStoreContent = driverStoreContents[i];
                    driverStoreContents.RemoveAt(i);

                    return driverStoreContent;
                }
            }

            while (this.directoryInfoEnumerator.MoveNext())
            {
                var item = this.directoryInfoEnumerator.Current;
                Match match = DriverStoreRepositoryDirNameRegex.Match(item.Name);
                if (match.Success)
                {
                    string infName = match.Groups[1].Value;
                    string infPath = Path.Combine(item.FullName, infName);
                    if (File.Exists(infPath))
                    {
                        string infContent = File.ReadAllText(infPath);
                        long estimateSize = GetFolderSize(item);

                        DriverStoreContent driverStoreContent = new DriverStoreContent()
                        {
                            InfName = infName,
                            Content = infContent,
                            EstimateSize = estimateSize
                        };

                        if (infContent == content)
                        {
                            return driverStoreContent;
                        }
                        else
                        {
                            driverStoreContents.Add(driverStoreContent);
                        }
                    }
                }
            }

            return null;
        }

        private static string GetSystemRootInfContent(string infName)
        {
            string content = null;
            string filePath = Path.Combine(SystemRootInf, infName);
            if (File.Exists(filePath))
            {
                content = File.ReadAllText(filePath);
            }

            return content;
        }

        private static long GetFolderSize(DirectoryInfo directory)
        {
            long size = 0;

            // Add file sizes.
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                size += fileInfo.Length;
            }

            // Add subdirectory sizes.
            foreach (DirectoryInfo dirInfo in directory.GetDirectories())
            {
                size += GetFolderSize(dirInfo);
            }

            return size;
        }
    }
}
