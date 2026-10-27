using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rapr.Tests
{
    [TestClass]
    public class DSEFormHelperTests
    {
        private static readonly Func<string, string> RemoveExtendedPathPrefix =
            (Func<string, string>)typeof(DSEFormHelper)
                .GetMethod("RemoveExtendedPathPrefix", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<string, string>));

        [TestMethod]
        [DataRow(@"\\?\C:\Tools\Rapr.exe", @"C:\Tools\Rapr.exe")]
        [DataRow(@"\\?\UNC\server\share\Rapr.exe", @"\\server\share\Rapr.exe")]
        [DataRow(@"C:\Tools\Rapr.exe", @"C:\Tools\Rapr.exe")]
        public void ExtendedPathPrefixIsRemoved(string path, string expected)
        {
            Assert.AreEqual(expected, RemoveExtendedPathPrefix(path));
        }
    }
}
