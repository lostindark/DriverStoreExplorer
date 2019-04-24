using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Lang;

namespace Rapr.Tests
{
    [TestClass]
    public class RaprTests
    {
        [TestMethod]
        public void TestEnglish()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("EN");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Assert.IsTrue(Language.UnitTest_Language == "English");
        }

        [TestMethod]
        public void TestFrench()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Assert.IsTrue(Language.UnitTest_Language == "Français");
        }
    }
}
