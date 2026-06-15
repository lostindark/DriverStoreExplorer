using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class WinPEInfDetectorTests
    {
        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenBootFlagsContainsWinPEFlag()
        {
            const string inf = @"
[MyDriver_Service_Inst]
BootFlags=0x80
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenBootFlagsIsOrCombination()
        {
            const string inf = @"
[MyDriver_Service_Inst]
BootFlags=0x81
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsFalse_WhenBootFlagsPresentWithoutWinPEFlag()
        {
            const string inf = @"
[MyDriver_Service_Inst]
BootFlags=0x01
";

            Assert.AreEqual(false, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsNull_WhenNoIndicatorsPresent()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
";

            Assert.IsNull(WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenAddRegBootFlagsContainsWinPEFlag()
        {
            const string inf = @"
[MyDriver_Service_Inst]
AddReg = MyDriver_AddReg

[MyDriver_AddReg]
HKR,,BootFlags,0x00010001,0x80
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenClassIsNet()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
Class=Net
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenClassIsResolvedFromStrings()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
Class=%ClassName%

[Strings]
ClassName=""SCSIAdapter""
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsFalse_WhenClassIsPrinter()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
Class=Printer
";

            Assert.AreEqual(false, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsFalse_WhenCoInstallersSectionPresent()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
Class=Net

[MyDevice.NT.CoInstallers32]
AddReg=MyCoInstaller_AddReg
";

            Assert.AreEqual(false, WinPEInfDetector.DetectFromInfContent(inf));
        }

        [TestMethod]
        public void DetectFromInfContent_ReturnsTrue_WhenBootFlagsWinPEOverridesExcludedClass()
        {
            const string inf = @"
[Version]
Signature=""$Windows NT$""
Class=Printer

[MyDriver_Service_Inst]
BootFlags=0x80
";

            Assert.AreEqual(true, WinPEInfDetector.DetectFromInfContent(inf));
        }
    }
}