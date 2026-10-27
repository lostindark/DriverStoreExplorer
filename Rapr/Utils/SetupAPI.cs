using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Rapr.Utils
{
    public static class SetupAPI
    {
        public static string GetDriverSignerInfo(string driverStoreFilename)
        {
            const int ERROR_AUTHENTICODE_TRUSTED_PUBLISHER = unchecked((int)0xe0000241);
            const int ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED = unchecked((int)0xe0000242);

            SP_INF_SIGNER_INFO signerInfo = new SP_INF_SIGNER_INFO
            {
                cbSize = Marshal.SizeOf(typeof(SP_INF_SIGNER_INFO))
            };

            bool verifyResult = NativeMethods.SetupVerifyInfFile(driverStoreFilename, IntPtr.Zero, ref signerInfo);
            int lastError = Marshal.GetLastWin32Error();

            if ((verifyResult
                || lastError == ERROR_AUTHENTICODE_TRUSTED_PUBLISHER
                || lastError == ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED)
                && !string.IsNullOrEmpty(signerInfo.DigitalSigner))
            {
                return signerInfo.DigitalSigner;
            }
            else
            {
                return string.Empty;
            }
        }

        public static void AddDriver(string infFullPath, bool install)
        {
            bool result;

            if (install)
            {
                result = NativeMethods.DiInstallDriver(IntPtr.Zero, infFullPath, 0, out _);
            }
            else
            {
                StringBuilder destInfFullPath = new StringBuilder(MAX_PATH);
                uint requiredSize = 0;

                result = NativeMethods.SetupCopyOEMInf(
                    infFullPath,
                    null,
                    OemSourceMediaType.SPOST_PATH,
                    OemCopyStyle.NONE,
                    destInfFullPath,
                    (uint)destInfFullPath.Capacity,
                    ref requiredSize,
                    IntPtr.Zero);
            }

            if (!result)
            {
                throw new Win32Exception();
            }
        }

        public static void DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            if (driverStoreEntry == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntry));
            }

            if (forceDelete
                && MethodExists("newdev.dll", "DiUninstallDriverW")
                && !NativeMethods.DiUninstallDriver(
                        IntPtr.Zero,
                        Path.Combine(driverStoreEntry.DriverFolderLocation, driverStoreEntry.DriverInfName),
                        DIURFLAG.NO_REMOVE_INF,
                        out _))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.SetupUninstallOEMInf(
                driverStoreEntry.DriverPublishedName,
                forceDelete ? SetupUOInfFlags.SUOI_FORCEDELETE : SetupUOInfFlags.NONE,
                IntPtr.Zero))
            {
                throw new Win32Exception();
            }
        }

        public static bool MethodExists(string libraryName, string methodName)
        {
            var libraryPtr = NativeMethods.LoadLibrary(libraryName);

            if (libraryPtr != UIntPtr.Zero)
            {
                var procPtr = NativeMethods.GetProcAddress(libraryPtr, methodName);
                return procPtr != UIntPtr.Zero;
            }

            return false;
        }

        internal const int MAX_PATH = 260;

        /// <summary>
        /// Flags for DiUninstallDriver
        /// </summary>
        [Flags]
        internal enum DIURFLAG
        {
            NO_REMOVE_INF = 0x00000001, // Do not remove inf from the system
            UNCONFIGURE_INF = 0x00000002, // Unconfigure inf, if possible
        }

        [Flags]
        internal enum SetupUOInfFlags : uint
        {
            NONE = 0x0000,
            SUOI_FORCEDELETE = 0x0001
        };

        /// <summary>
        /// Driver media type
        /// </summary>
        internal enum OemSourceMediaType
        {
            SPOST_NONE = 0,
            SPOST_PATH = 1,
            SPOST_URL = 2,
            SPOST_MAX = 3
        }

        /// <summary>
        /// Driver file copy style
        /// </summary>
        internal enum OemCopyStyle
        {
            NONE = 0x0,
            SP_COPY_DELETESOURCE = 0x0000001,   // delete source file on successful copy
            SP_COPY_REPLACEONLY = 0x0000002,   // copy only if target file already present
            SP_COPY_NEWER = 0x0000004,   // copy only if source newer than or same as target
            SP_COPY_NEWER_OR_SAME = SP_COPY_NEWER,
            SP_COPY_NOOVERWRITE = 0x0000008,   // copy only if target doesn't exist
            SP_COPY_NODECOMP = 0x0000010,   // don't decompress source file while copying
            SP_COPY_LANGUAGEAWARE = 0x0000020,   // don't overwrite file of different language
            SP_COPY_SOURCE_ABSOLUTE = 0x0000040,   // SourceFile is a full source path
            SP_COPY_SOURCEPATH_ABSOLUTE = 0x0000080,   // SourcePathRoot is the full path
            SP_COPY_IN_USE_NEEDS_REBOOT = 0x0000100,   // System needs reboot if file in use
            SP_COPY_FORCE_IN_USE = 0x0000200,   // Force target-in-use behavior
            SP_COPY_NOSKIP = 0x0000400,   // Skip is disallowed for this file or section
            SP_FLAG_CABINETCONTINUATION = 0x0000800,   // Used with need media notification
            SP_COPY_FORCE_NOOVERWRITE = 0x0001000,   // like NOOVERWRITE but no callback nofitication
            SP_COPY_FORCE_NEWER = 0x0002000,   // like NEWER but no callback nofitication
            SP_COPY_WARNIFSKIP = 0x0004000,   // system critical file: warn if user tries to skip
            SP_COPY_NOBROWSE = 0x0008000,   // Browsing is disallowed for this file or section
            SP_COPY_NEWER_ONLY = 0x0010000,   // copy only if source file newer than target
            SP_COPY_SOURCE_SIS_MASTER = 0x0020000,   // source is single-instance store master
            SP_COPY_OEMINF_CATALOG_ONLY = 0x0040000,   // (SetupCopyOEMInf only) don't copy INF--just catalog
            SP_COPY_REPLACE_BOOT_FILE = 0x0080000,   // file must be present upon reboot (i.e., it's needed by the loader), this flag implies a reboot
            SP_COPY_NOPRUNE = 0x0100000   // never prune this file
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_INF_SIGNER_INFO
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string CatalogFile;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DigitalSigner;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DigitalSignerVersion;
            public int SignerScore;
        };

        /// <summary>
        /// The managed interop layer to setupapi.dll
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern UIntPtr LoadLibrary(string lpFileName);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "GetProcAddress only comes in an ANSI flavor.")]
            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
            internal static extern UIntPtr GetProcAddress(UIntPtr hModule, string lpProcName);

            [DllImport("newdev.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool DiInstallDriver(
                [In] IntPtr hwndParent,
                [In] string infPath,
                [In] uint flags,
                [Out] out bool needReboot
            );

            [DllImport("newdev.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool DiUninstallDriver(
                [In] IntPtr hwndParent,
                [In] string infPath,
                [In] DIURFLAG flags,
                [Out] out bool needReboot
            );

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupVerifyInfFile(
                string infName,
                IntPtr altPlatformInfo,
                ref SP_INF_SIGNER_INFO infSignerInfo);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupCopyOEMInf(
                [MarshalAs(UnmanagedType.LPWStr)] string infName,
                [MarshalAs(UnmanagedType.LPWStr)] string sourceMediaLocation,
                OemSourceMediaType sourceMediaType,
                OemCopyStyle copyStyle,
                [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder destInfFullPath,
                uint destInfFullPathSize,
                ref uint desInfFullPathReqSize,
                IntPtr destInfFilename);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupUninstallOEMInf(
                [MarshalAs(UnmanagedType.LPWStr)] string infName,
                SetupUOInfFlags flags,
                IntPtr reserved);
        }
    }
}
