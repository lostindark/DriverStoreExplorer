using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Rapr.Utils
{
    public class DriverStoreContent
    {
        public string InfName { get; set; }

        public string FolderPath { get; set; }

        public string Content { get; set; }

        public long EstimateSize { get; set; }
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

        public void FindInfInfo(string infName, out string originalInfName, out string driverFolderLocation, out long estimateSize)
        {
            originalInfName = "[Unknown]";
            driverFolderLocation = string.Empty;
            estimateSize = -1;

            try
            {
                if (!string.IsNullOrEmpty(infName))
                {
                    string content = GetSystemRootInfContent(infName);

                    if (!string.IsNullOrEmpty(content))
                    {
                        DriverStoreContent driverStoreContent = this.FindInfInfo(content);

                        if (driverStoreContent != null)
                        {
                            originalInfName = driverStoreContent.InfName;
                            driverFolderLocation = driverStoreContent.FolderPath;
                            estimateSize = driverStoreContent.EstimateSize;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore this entry if we don't have access to it.
            }
        }

        private DriverStoreContent FindInfInfo(string content)
        {
            for (int i = 0; i < this.driverStoreContents.Count; i++)
            {
                if (this.driverStoreContents[i].Content == content)
                {
                    DriverStoreContent driverStoreContent = this.driverStoreContents[i];
                    this.driverStoreContents.RemoveAt(i);

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
                        try
                        {
                            string infContent = File.ReadAllText(infPath);
                            long estimateSize = GetFolderSize(item);

                            DriverStoreContent driverStoreContent = new DriverStoreContent()
                            {
                                InfName = infName,
                                FolderPath = item.FullName,
                                Content = infContent,
                                EstimateSize = estimateSize
                            };

                            if (infContent == content)
                            {
                                return driverStoreContent;
                            }
                            else
                            {
                                this.driverStoreContents.Add(driverStoreContent);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Ignore this entry if we don't have access to it.
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

        public static long GetFolderSize(DirectoryInfo directory)
        {
            long size = 0;

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

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
