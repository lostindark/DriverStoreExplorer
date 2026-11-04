using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Microsoft.Win32.SafeHandles;

namespace Rapr
{
    /// <summary>
    /// This class suppresses stack walks for unmanaged code permission. 
    /// (System.Security.SuppressUnmanagedCodeSecurityAttribute is applied to this class.) 
    /// This class is for methods that are safe for anyone to call. 
    /// Callers of these methods are not required to perform a full security review to make sure that the 
    /// usage is secure because the methods are harmless for any caller.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class SafeNativeMethods
    {
        [DllImport("shell32.dll", EntryPoint = "ExtractAssociatedIcon", CharSet = CharSet.Unicode)]
        internal static extern IntPtr ExtractAssociatedIcon(HandleRef hInst, StringBuilder iconPath, ref int index);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HideCaret(HandleRef hWnd);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
        internal static extern IntPtr SendMessage(
            HandleRef hWnd, int message, IntPtr wParam, ref ParagraphFormat2 format);

        // PARAFORMAT2; the remaining fields are unused when only paragraph spacing is set.
        [StructLayout(LayoutKind.Explicit, Size = 188)]
        internal struct ParagraphFormat2
        {
            [FieldOffset(0)]
            internal uint Size;

            [FieldOffset(4)]
            internal uint Mask;

            [FieldOffset(156)]
            internal int SpaceBefore;

            [FieldOffset(160)]
            internal int SpaceAfter;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetFinalPathNameByHandle(
            SafeFileHandle hFile,
            StringBuilder lpszFilePath,
            int cchFilePath,
            int dwFlags);
    }
}
