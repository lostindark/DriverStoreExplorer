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
            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.Name);
            ci = CultureInfo.GetCultureInfo("EN");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Assert.IsTrue(Language.UnitTest_Language == "English");
        }

        [TestMethod]
        public void TestFrench()
        {
            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.Name);
            ci = CultureInfo.GetCultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Assert.IsTrue(Language.UnitTest_Language == "Français");
        }
    }
}
