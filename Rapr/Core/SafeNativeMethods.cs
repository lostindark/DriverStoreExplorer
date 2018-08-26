using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

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
    }
}
