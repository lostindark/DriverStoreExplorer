using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Rapr.Utils
{
    /// <summary>
    /// Detects WinPE compatibility from INF content using BootFlags (0x80), device class, and co-installer checks.
    /// </summary>
    public static class WinPEInfDetector
    {
        private const int CmServiceWinPEBootLoad = 0x80;

        private static readonly HashSet<string> WinPEUsefulClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SCSIAdapter",
            "HDC",
            "Net",
            "System",
            "DiskDrive",
        };

        private static readonly HashSet<string> WinPEExcludedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Media",
            "Printer",
            "Image",
            "Biometric",
        };

        private static readonly Regex BootFlagsDirectiveRegex = new Regex(
            @"^\s*BootFlags\s*=\s*(.+?)\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BootFlagsAddRegRegex = new Regex(
            @"BootFlags\s*,\s*0x[0-9A-Fa-f]+\s*,\s*(?:0x)?([0-9A-Fa-f]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InfClassRegex = new Regex(
            @"^Class\s*=\s*([^\r\n;]+)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CoInstallersSectionRegex = new Regex(
            @"^\[[^\]]*\.CoInstallers32\]",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InfStringsLineRegex = new Regex(
            @"^([^=;\r\n\[]+)\s*=\s*(.*)$",
            RegexOptions.Compiled);

        public static bool? DetectFromInfFile(string infFilePath)
        {
            if (string.IsNullOrEmpty(infFilePath) || !File.Exists(infFilePath))
            {
                return null;
            }

            try
            {
                return DetectFromInfContent(File.ReadAllText(infFilePath));
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Failed to read INF for WinPE detection: {infFilePath}: {ex.Message}");
                return null;
            }
        }

        public static bool? DetectFromInfContent(string infContent)
        {
            if (string.IsNullOrEmpty(infContent))
            {
                return null;
            }

            if (HasWinPEBootFlag(infContent))
            {
                return true;
            }

            bool bootFlagsWithoutWinPE = HasBootFlagsWithoutWinPE(infContent);
            string driverClass = ParseInfClass(infContent);

            if (!string.IsNullOrEmpty(driverClass) && WinPEExcludedClasses.Contains(driverClass))
            {
                return false;
            }

            if (CoInstallersSectionRegex.IsMatch(infContent))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(driverClass) && WinPEUsefulClasses.Contains(driverClass))
            {
                return true;
            }

            if (bootFlagsWithoutWinPE)
            {
                return false;
            }

            return null;
        }

        private static bool HasWinPEBootFlag(string infContent)
        {
            foreach (Match match in BootFlagsDirectiveRegex.Matches(infContent))
            {
                if (TryParseBootFlagsValue(match.Groups[1].Value, out int flags)
                    && (flags & CmServiceWinPEBootLoad) != 0)
                {
                    return true;
                }
            }

            foreach (Match match in BootFlagsAddRegRegex.Matches(infContent))
            {
                if (int.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out int flags)
                    && (flags & CmServiceWinPEBootLoad) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasBootFlagsWithoutWinPE(string infContent)
        {
            bool foundBootFlags = false;

            foreach (Match match in BootFlagsDirectiveRegex.Matches(infContent))
            {
                if (!TryParseBootFlagsValue(match.Groups[1].Value, out int flags))
                {
                    continue;
                }

                foundBootFlags = true;
                if ((flags & CmServiceWinPEBootLoad) != 0)
                {
                    return false;
                }
            }

            foreach (Match match in BootFlagsAddRegRegex.Matches(infContent))
            {
                if (!int.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out int flags))
                {
                    continue;
                }

                foundBootFlags = true;
                if ((flags & CmServiceWinPEBootLoad) != 0)
                {
                    return false;
                }
            }

            return foundBootFlags;
        }

        private static string ParseInfClass(string infContent)
        {
            Match match = InfClassRegex.Match(infContent);
            if (!match.Success)
            {
                return string.Empty;
            }

            return ResolveInfToken(match.Groups[1].Value.Trim(), ParseStringsSection(infContent));
        }

        private static Dictionary<string, string> ParseStringsSection(string infContent)
        {
            var strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int stringsIdx = infContent.IndexOf("[Strings]", StringComparison.OrdinalIgnoreCase);
            if (stringsIdx < 0)
            {
                return strings;
            }

            string stringsArea = infContent.Substring(stringsIdx);
            foreach (var line in stringsArea.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) &&
                    !trimmed.StartsWith("[Strings]", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Match stringMatch = InfStringsLineRegex.Match(line);
                if (!stringMatch.Success)
                {
                    continue;
                }

                string key = stringMatch.Groups[1].Value.Trim();
                if (key.StartsWith("[", StringComparison.Ordinal))
                {
                    break;
                }

                string value = stringMatch.Groups[2].Value.Trim().Trim('"');
                if (!strings.ContainsKey(key))
                {
                    strings[key] = value;
                }
            }

            return strings;
        }

        private static string ResolveInfToken(string raw, IReadOnlyDictionary<string, string> strings)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            raw = raw.Trim().Trim('"');
            for (int i = 0; i < 5; i++)
            {
                if (!raw.StartsWith("%", StringComparison.Ordinal) || !raw.EndsWith("%", StringComparison.Ordinal))
                {
                    return raw;
                }

                string token = raw.Substring(1, raw.Length - 2).Trim();
                if (strings != null && strings.TryGetValue(token, out string resolved))
                {
                    raw = resolved.Trim().Trim('"');
                }
                else
                {
                    return token;
                }
            }

            return raw;
        }

        private static bool TryParseBootFlagsValue(string raw, out int flags)
        {
            flags = 0;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            raw = raw.Trim().TrimEnd(';');
            int commentIndex = raw.IndexOf(';');
            if (commentIndex >= 0)
            {
                raw = raw.Substring(0, commentIndex).Trim();
            }

            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(
                    raw.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out flags);
            }

            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out flags);
        }
    }
}