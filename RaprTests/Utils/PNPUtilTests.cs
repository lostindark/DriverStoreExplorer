using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass()]
    public class PnpUtilTests
    {
        private const string EnglishPnpUtilEnumerateOutput = @"Microsoft PnP Utility

Published name :            oem4.inf
Driver package provider :   Microsoft
Class :                     Human Interface Devices
Driver date and version :   11/06/2015 9.9.114.0
Signer name :               Microsoft Windows Hardware Compatibility Publisher";

        private const string EnglishPnpUtilEnumerateOutputWithoutSignerName = @"Microsoft PnP Utility

Published name :            oem4.inf
Driver package provider :   Microsoft
Class :                     Human Interface Devices
Driver date and version :   11/06/2015 9.9.114.0
Signer name :               ";

        private const string EnglishPnpUtilEnumerateOutputWithDummyLine = @"Microsoft PnP Utility

Published name :            oem4.inf
Driver package provider :   Microsoft
Class :                     
Human Interface Devices
Dummy line
Driver date and version :   11/06/2015 9.9.114.0
Signer name :               Microsoft Windows Hardware Compatibility Publisher";

        private const string EnglishPnpUtilEnumerateOutputWithMissingLine = @"Microsoft PnP Utility

Published name :            oem4.inf
Driver package provider :   Microsoft
Class :                     Human Interface Devices
Driver date and version :   11/06/2015 9.9.114.0

Published name :            oem18.inf
Driver package provider :   Intel
Class :                     System devices
Driver date and version :   12/17/2012 9.0.0.1287
Signer name :               Microsoft Windows Hardware Compatibility Publisher";

        private const string ChinesePnpUtilEnumerateOutput = @"Microsoft PnP 工具 

发布名称:             oem0.inf 
驱动程序程序包提供程序: Microsoft 
类:                     打印机 
驱动程序日期和版本: 06/21/2006 6.1.7600.16385 
签名人姓名:                Microsoft Windows";

        private const string LongChinesePnpUtilEnumerateOutput = @"Microsoft PnP 工具 

发布名称:             oem0.inf 
驱动程序程序包提供程序: Microsoft 
类:                     打印机 
驱动程序日期和版本: 06/21/2006 6.1.7600.16385 
签名人姓名:                Microsoft Windows 

发布名称:             oem1.inf 
驱动程序程序包提供程序: Microsoft 
类:                     打印机 
驱动程序日期和版本: 06/21/2006 6.1.7601.17514 
签名人姓名:                Microsoft Windows 

发布名称:             oem2.inf 
驱动程序程序包提供程序: Realtek Semiconductor Corp. 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 06/04/2012 6.0.1.6650 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem3.inf 
驱动程序程序包提供程序: Realtek Semiconductor Corp. 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 03/29/2013 6.0.1.6873 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem4.inf 
驱动程序程序包提供程序: Intel Corporation 
类:                     IDE ATA/ATAPI 控制器 
驱动程序日期和版本: 12/11/2012 11.7.1.1001 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem5.inf 
驱动程序程序包提供程序: Advanced Micro Devices, Inc. 
类:                     显示适配器 
驱动程序日期和版本: 03/28/2013 12.104.0.0000 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem6.inf 
驱动程序程序包提供程序: Advanced Micro Devices 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 03/01/2013 7.12.0.7714 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem7.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 03/27/2013 7.071.0327.2013 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem8.inf 
驱动程序程序包提供程序: Microsoft 
类:                     未知驱动程序类 
驱动程序日期和版本: 12/11/2002 9.0.0.2980 
签名人姓名:                Microsoft Windows Component Publisher 

发布名称:             oem9.inf 
驱动程序程序包提供程序: Microsoft 
类:                     未知驱动程序类 
驱动程序日期和版本: 11/06/2002 9.0.0.3274 
签名人姓名:                Microsoft Windows Component Publisher 

发布名称:             oem10.inf 
驱动程序程序包提供程序: Mingwah Aohan High Technology 
类:                     通用串行总线控制器 
驱动程序日期和版本: 04/16/2010 3.0.47.2600 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem20.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 02/25/2013 9.2.0.1033 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem12.inf 
驱动程序程序包提供程序: 江苏意源科技有限公司 
类:                     未知驱动程序类 
驱动程序日期和版本: 08/22/2002 1.0.5.0 
签名人姓名:                 

发布名称:             oem21.inf 
驱动程序程序包提供程序: Intel(R) Corporation 
类:                     通用串行总线控制器 
驱动程序日期和版本: 12/04/2012 1.0.7.248 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem30.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 01/08/2014 7.079.0108.2014 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem13.inf 
驱动程序程序包提供程序: HTC 
类:                     网络协议 
驱动程序日期和版本: 06/25/2010 1.0.0.1 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem22.inf 
驱动程序程序包提供程序: Intel(R) Corporation 
类:                     通用串行总线控制器 
驱动程序日期和版本: 12/04/2012 1.0.7.248 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem31.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 08/27/2013 7.075.0827.2013 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem40.inf 
驱动程序程序包提供程序: Intel 
类:                     端口 (COM 和 LPT) 
驱动程序日期和版本: 01/23/2014 10.0.0.1096 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem14.inf 
驱动程序程序包提供程序: Realtek Semiconductor Corp. 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 03/29/2013 6.0.1.6873 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem23.inf 
驱动程序程序包提供程序: 英特尔 
类:                     系统设备 
驱动程序日期和版本: 12/04/2012 1.0.7.248 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem32.inf 
驱动程序程序包提供程序: Advanced Micro Devices 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 09/21/2012 7.12.0.7712 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem41.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 04/24/2014 10.0.16 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem50.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 05/02/2014 12.10.29.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem15.inf 
驱动程序程序包提供程序: Etron Tech 
类:                     图像设备 
驱动程序日期和版本: 07/12/2011 1.0.3.6 
签名人姓名:                 

发布名称:             oem24.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 12/17/2012 9.0.0.1287 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem33.inf 
驱动程序程序包提供程序: Philips Electronics 
类:                     监视器 
驱动程序日期和版本: 07/11/2008 1.0.0.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem42.inf 
驱动程序程序包提供程序: VIA 
类:                     网络适配器 
驱动程序日期和版本: 03/31/2011 1.15.0.1 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem51.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 06/12/2014 12.12.50.4 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem60.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 07/11/2014 3.9.58.9101 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem16.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 11/19/2012 1.0.8.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem25.inf 
驱动程序程序包提供程序: Intel Corporation 
类:                     IDE ATA/ATAPI 控制器 
驱动程序日期和版本: 02/26/2014 12.9.2.1000 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem34.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 10/28/2013 7.076.1028.2013 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem43.inf 
驱动程序程序包提供程序: Google, Inc. 
类:                     Android Tablet 
驱动程序日期和版本: 12/06/2010 4.0.0000.00000 
签名人姓名:                 

发布名称:             oem52.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 06/19/2013 9.16.10.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem61.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 10/13/2014 1.0.114.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem70.inf 
驱动程序程序包提供程序: SAMSUNG Electronics Co., Ltd. 
类:                     通用串行总线控制器 
驱动程序日期和版本: 02/16/2012 2.9.317.0215 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem17.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 02/25/2013 9.3.0.1027 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem26.inf 
驱动程序程序包提供程序: Intel(R) Corporation 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 06/19/2012 6.14.00.3097 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem35.inf 
驱动程序程序包提供程序: Intel Corporation 
类:                     显示适配器 
驱动程序日期和版本: 10/31/2013 9.17.10.3347 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem44.inf 
驱动程序程序包提供程序: Huawei Incorporated 
类:                     端口 (COM 和 LPT) 
驱动程序日期和版本: 04/20/2012 1.03.00.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem53.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 07/18/2013 12.10.13.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem62.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 11/16/2009 1.3.22.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem71.inf 
驱动程序程序包提供程序: SAMSUNG Electronics Co., Ltd. 
类:                     调制解调器 
驱动程序日期和版本: 01/02/2014 2.11.7.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem80.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     通用串行总线控制器 
驱动程序日期和版本: 03/01/2016 6.14.13.6444 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem18.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 02/25/2013 9.3.0.1027 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem27.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 04/10/2013 7.072.0410.2013 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem36.inf 
驱动程序程序包提供程序: Realtek 
类:                     网络适配器 
驱动程序日期和版本: 02/18/2014 7.080.0218.2014 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem45.inf 
驱动程序程序包提供程序: Huawei Incorporated 
类:                     端口 (COM 和 LPT) 
驱动程序日期和版本: 12/28/2015 1.03.00.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem54.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 05/16/2013 12.7.27.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem63.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 11/16/2009 1.3.22.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem72.inf 
驱动程序程序包提供程序: Microsoft 
类:                     打印机 
驱动程序日期和版本: 04/29/2013 16.0.1626.4000 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem81.inf 
驱动程序程序包提供程序: Google, Inc. 
类:                     Android Tablet 
驱动程序日期和版本: 12/06/2010 4.0.0000.00000 
签名人姓名:                 

发布名称:             oem90.inf 
驱动程序程序包提供程序: HUAWEI Incorporated 
类:                     网络适配器 
驱动程序日期和版本: 06/27/2014 1.03.00.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem19.inf 
驱动程序程序包提供程序: Intel 
类:                     通用串行总线控制器 
驱动程序日期和版本: 02/25/2013 9.3.0.1027 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem28.inf 
驱动程序程序包提供程序: Advanced Micro Devices, Inc. 
类:                     显示适配器 
驱动程序日期和版本: 08/30/2013 13.152.0.0000 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem37.inf 
驱动程序程序包提供程序: SoftEther VPN Project 
类:                     网络适配器 
驱动程序日期和版本: 04/09/2014 4.6.0.9437 
签名人姓名:                 

发布名称:             oem46.inf 
驱动程序程序包提供程序: Huawei Incorporated 
类:                     调制解调器 
驱动程序日期和版本: 04/08/2015 1.03.00.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem55.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 05/15/2014 12.11.97.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem64.inf 
驱动程序程序包提供程序: Intel 
类:                     系统设备 
驱动程序日期和版本: 06/13/2009 1.3.12.3 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem73.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     显示适配器 
驱动程序日期和版本: 11/24/2015 10.18.13.5906 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem82.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 04/12/2016 1.2.40 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem91.inf 
驱动程序程序包提供程序: MediaTek Inc. 
类:                     端口 (COM 和 LPT) 
驱动程序日期和版本: 11/06/2014 2.0.1136.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem29.inf 
驱动程序程序包提供程序: Advanced Micro Devices, Inc. 
类:                     显示适配器 
驱动程序日期和版本: 12/19/2012 9.012.0.0000 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem38.inf 
驱动程序程序包提供程序: SoftEther VPN Project 
类:                     网络适配器 
驱动程序日期和版本: 04/09/2014 4.6.0.9437 
签名人姓名:                 

发布名称:             oem47.inf 
驱动程序程序包提供程序: Google, Inc. 
类:                     Android Tablet 
驱动程序日期和版本: 06/30/2015 2.00.03.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem56.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 03/06/2014 12.11.95.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem65.inf 
驱动程序程序包提供程序: Realtek Semiconductor Corp. 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 01/31/2012 6.0.1.6559 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem74.inf 
驱动程序程序包提供程序: NVIDIA Corporation 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 09/21/2015 1.3.34.4 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem83.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     显示适配器 
驱动程序日期和版本: 05/19/2016 10.18.13.6822 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem92.inf 
驱动程序程序包提供程序: Huawei 
类:                     通用串行总线控制器 
驱动程序日期和版本: 12/28/2015 1.0.10.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem39.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 08/10/2012 12.2.45.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem48.inf 
驱动程序程序包提供程序: ClockworkMod 
类:                     Android Tablet 
驱动程序日期和版本: 08/27/2012 7.0.0000.00004 
签名人姓名:                 

发布名称:             oem57.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 10/20/2011 10.1.17.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem66.inf 
驱动程序程序包提供程序: Marvell 
类:                     打印机 
驱动程序日期和版本: 04/15/2013 1.0.2.2680 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem75.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     通用串行总线控制器 
驱动程序日期和版本: 04/27/2015 6.14.13.5265 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem84.inf 
驱动程序程序包提供程序: NVIDIA Corporation 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 03/24/2016 1.3.34.14 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem93.inf 
驱动程序程序包提供程序: HUAWEI Technologies CO.,LTD 
类:                     端口 (COM 和 LPT) 
驱动程序日期和版本: 12/18/2015 1.0.24.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem49.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 10/01/2009 9.0.13.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem58.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 05/15/2013 2.4.36.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem67.inf 
驱动程序程序包提供程序: Hewlett-Packard, Inc. 
类:                     通用串行总线控制器 
驱动程序日期和版本: 08/21/2012 15.57.47.479 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem76.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 08/10/2015 1.2.31 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem85.inf 
驱动程序程序包提供程序: Google, Inc. 
类:                     Android Tablet 
驱动程序日期和版本: 12/28/2015 2.00.03.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem94.inf 
驱动程序程序包提供程序: HUAWEI Technologies Co.,LTD 
类:                     调制解调器 
驱动程序日期和版本: 12/18/2015 1.0.24.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem59.inf 
驱动程序程序包提供程序: Intel 
类:                     网络适配器 
驱动程序日期和版本: 07/11/2014 3.9.58.9101 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem68.inf 
驱动程序程序包提供程序: Marvell 
类:                     打印机 
驱动程序日期和版本: 08/21/2012 5.0.1.56496 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem77.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     鼠标和其他指针设备 
驱动程序日期和版本: 08/25/2014 8.16.21500.1063 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem86.inf 
驱动程序程序包提供程序: VMware, Inc. 
类:                     通用串行总线控制器 
驱动程序日期和版本: 08/19/2015 4.1.1.1 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem95.inf 
驱动程序程序包提供程序: Microsoft 
类:                     便携设备 
驱动程序日期和版本: 07/29/2015 1.03.00.00 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem69.inf 
驱动程序程序包提供程序: Western Digital Technologies 
类:                     WD Drive Management devices 
驱动程序日期和版本: 04/28/2015 1.0.0017.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem78.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     声音、视频和游戏控制器 
驱动程序日期和版本: 03/13/2016 1.2.37 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem87.inf 
驱动程序程序包提供程序: VMware, Inc. 
类:                     Network Service 
驱动程序日期和版本: 01/23/2013 4.2.3.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem79.inf 
驱动程序程序包提供程序: NVIDIA 
类:                     显示适配器 
驱动程序日期和版本: 04/27/2016 10.18.13.6510 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem88.inf 
驱动程序程序包提供程序: VMware, Inc. 
类:                     网络适配器 
驱动程序日期和版本: 01/23/2014 4.2.3.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher 

发布名称:             oem89.inf 
驱动程序程序包提供程序: VMware, Inc. 
类:                     系统设备 
驱动程序日期和版本: 08/14/2014 9.7.1.0 
签名人姓名:                Microsoft Windows Hardware Compatibility Publisher";

        private const string RussianPnpUtilEnumerateOutput = @"Служебная программа PnP Майкрософт

Опубликованное имя :            
oem0.inf
Поставщик пакета драйвера:   Cisco Systems
Класс:                     Сетевые адаптеры
Дата разработки и версия драйвера :   
02/26/2014 3.1.6019.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher";

        private const string LongRussianPnpUtilEnumerateOutput = @"Служебная программа PnP Майкрософт

Опубликованное имя :            
oem0.inf
Поставщик пакета драйвера:   Cisco Systems
Класс:                     Сетевые адаптеры
Дата разработки и версия драйвера :   
02/26/2014 3.1.6019.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem1.inf
Поставщик пакета драйвера:   Microsoft
Класс:                     Принтеры
Дата разработки и версия драйвера :   
06/21/2006 6.1.7601.17514
Имя подписавшего :               Microsoft Windows

Опубликованное имя :            
oem2.inf
Поставщик пакета драйвера:   SCM Microsystems Inc.
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
01/24/2007 2.01.00.01
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem3.inf
Поставщик пакета драйвера:   SCM Microsystems Inc.
Класс:                     Устройства чтения смарт-карт
Дата разработки и версия драйвера :   
04/25/2007 4.39.00.01
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem4.inf
Поставщик пакета драйвера:   SeriousBit
Класс:                     Сетевая служба
Дата разработки и версия драйвера :   
02/02/2015 3.1.2
Имя подписавшего :               

Опубликованное имя :            
oem5.inf
Поставщик пакета драйвера:   Realtek Semiconductor Corp.
Класс:                     Звуковые, видео и игровые устройства
Дата разработки и версия драйвера :   
06/18/2015 6.0.1.7541
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem6.inf
Поставщик пакета драйвера:   Realtek
Класс:                     Сетевые адаптеры
Дата разработки и версия драйвера :   
12/26/2012 7.067.1226.2012
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem7.inf
Поставщик пакета драйвера:   NT Kernel Resources
Класс:                     Сетевая служба
Дата разработки и версия драйвера :   
02/21/2013 3.0.8.1
Имя подписавшего :               

Опубликованное имя :            
oem8.inf
Поставщик пакета драйвера:   ASUSTek Computer Inc
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
03/28/2013 5.30.20.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem9.inf
Поставщик пакета драйвера:   ASUSTek Computer Inc
Класс:                     Контроллеры запоминающих устройств
Дата разработки и версия драйвера :   
03/28/2013 5.30.20.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem10.inf
Поставщик пакета драйвера:   ASUSTek Computer Inc
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
03/28/2013 5.30.20.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem11.inf
Поставщик пакета драйвера:   MediaTek Inc.
Класс:                     Порты (COM и LPT)
Дата разработки и версия драйвера :   
01/22/2015 3.0.1504.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem20.inf
Поставщик пакета драйвера:   Intel
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
07/31/2013 9.4.0.1025
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem12.inf
Поставщик пакета драйвера:   NVIDIA
Класс:                     Видеоадаптеры
Дата разработки и версия драйвера :   
06/02/2016 10.18.13.6839
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem21.inf
Поставщик пакета драйвера:   Intel(R) Corporation
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
03/29/2013 2.5.0.19
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem30.inf
Поставщик пакета драйвера:   NVIDIA Corporation
Класс:                     Звуковые, видео и игровые устройства
Дата разработки и версия драйвера :   
05/10/2016 1.3.34.15
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem13.inf
Поставщик пакета драйвера:   NVIDIA Corporation
Класс:                     Звуковые, видео и игровые устройства
Дата разработки и версия драйвера :   
03/24/2016 1.3.34.14
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem22.inf
Поставщик пакета драйвера:   Intel(R) Corporation
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
03/29/2013 2.5.0.19
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem14.inf
Поставщик пакета драйвера:   NVIDIA
Класс:                     Контроллеры USB
Дата разработки и версия драйвера :   
03/01/2016 6.14.13.6444
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem23.inf
Поставщик пакета драйвера:   Intel
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
03/29/2013 2.5.0.19
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem15.inf
Поставщик пакета драйвера:   Intel
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
08/19/2013 9.5.14.1724
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem24.inf
Поставщик пакета драйвера:   ASUSTeK Computer Inc.
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
11/08/2012 12.14.17.909
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem42.inf
Поставщик пакета драйвера:   Logitech, Inc.
Класс:                     Звуковые, видео и игровые устройства
Дата разработки и версия драйвера :   
07/09/2014 8.53.0.2
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem16.inf
Поставщик пакета драйвера:   Intel
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
07/25/2013 9.4.0.1023
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem25.inf
Поставщик пакета драйвера:   SteelSeries
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
06/25/2013 2.4.0003.2
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem43.inf
Поставщик пакета драйвера:   Logitech
Класс:                     Неизвестен класс драйвера.
Дата разработки и версия драйвера :   
07/01/2009 3.03.107.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem17.inf
Поставщик пакета драйвера:   Intel
Класс:                     IDE ATA/ATAPI контроллеры
Дата разработки и версия драйвера :   
07/25/2013 9.4.0.1023
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem26.inf
Поставщик пакета драйвера:   SteelSeries
Класс:                     Устройства HID (Human Interface Devices)
Дата разработки и версия драйвера :   
06/25/2013 2.4.0003.2
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem35.inf
Поставщик пакета драйвера:   Microsoft
Класс:                     Принтеры
Дата разработки и версия драйвера :   
05/28/2012 15.0.4128.4000
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem44.inf
Поставщик пакета драйвера:   Logitech
Класс:                     Мыши и иные указывающие устройства
Дата разработки и версия драйвера :   
09/24/2014 8.57.00.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem18.inf
Поставщик пакета драйвера:   Intel
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
07/25/2013 9.4.0.1023
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem27.inf
Поставщик пакета драйвера:   NVIDIA
Класс:                     Звуковые, видео и игровые устройства
Дата разработки и версия драйвера :   
04/12/2016 1.2.40
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem45.inf
Поставщик пакета драйвера:   Logitech
Класс:                     Устройства HID (Human Interface Devices)
Дата разработки и версия драйвера :   
04/19/2013 8.46.17.1
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem19.inf
Поставщик пакета драйвера:   Intel
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
07/25/2013 9.4.0.1023
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem28.inf
Поставщик пакета драйвера:   NVIDIA
Класс:                     Мыши и иные указывающие устройства
Дата разработки и версия драйвера :   
08/25/2014 8.16.21500.1063
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem46.inf
Поставщик пакета драйвера:   Logitech
Класс:                     Системные устройства
Дата разработки и версия драйвера :   
11/23/2009 3.04.131.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem29.inf
Поставщик пакета драйвера:   NVIDIA
Класс:                     Видеоадаптеры
Дата разработки и версия драйвера :   
06/29/2016 10.18.13.6869
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher

Опубликованное имя :            
oem47.inf
Поставщик пакета драйвера:   Logitech
Класс:                     Устройства HID (Human Interface Devices)
Дата разработки и версия драйвера :   
11/23/2009 3.04.131.0
Имя подписавшего :               Microsoft Windows Hardware Compatibility Publisher
";

        [TestMethod()]
        public void ParseEnglishPnpUtilEnumerateResultTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(EnglishPnpUtilEnumerateOutput);

            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Count);

            Assert.AreEqual("oem4.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Microsoft", entries[0].DriverPkgProvider);
            Assert.AreEqual("Human Interface Devices", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2015, 11, 06, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(9, 9, 114, 0), entries[0].DriverVersion);
            Assert.AreEqual("Microsoft Windows Hardware Compatibility Publisher", entries[0].DriverSignerName);
        }

        [TestMethod()]
        public void ParseEnglishPnpUtilEnumerateResultWithoutSignerNameTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(EnglishPnpUtilEnumerateOutputWithoutSignerName);

            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Count);

            Assert.AreEqual("oem4.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Microsoft", entries[0].DriverPkgProvider);
            Assert.AreEqual("Human Interface Devices", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2015, 11, 06, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(9, 9, 114, 0), entries[0].DriverVersion);
            Assert.AreEqual(null, entries[0].DriverSignerName);
        }

        [TestMethod()]
        public void ParseEnglishPnpUtilEnumerateResultWithDummyLineTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(EnglishPnpUtilEnumerateOutputWithDummyLine);

            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Count);

            Assert.AreEqual("oem4.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Microsoft", entries[0].DriverPkgProvider);
            Assert.AreEqual("Human Interface Devices", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2015, 11, 06, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(9, 9, 114, 0), entries[0].DriverVersion);
            Assert.AreEqual("Microsoft Windows Hardware Compatibility Publisher", entries[0].DriverSignerName);
        }

        [TestMethod()]
        public void ParseEnglishPnpUtilEnumerateResultWithMissingLineTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(EnglishPnpUtilEnumerateOutputWithMissingLine);

            Assert.IsNotNull(entries);
            Assert.AreEqual(2, entries.Count);

            Assert.AreEqual("oem4.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Microsoft", entries[0].DriverPkgProvider);
            Assert.AreEqual("Human Interface Devices", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2015, 11, 06, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(9, 9, 114, 0), entries[0].DriverVersion);
            Assert.AreEqual(null, entries[0].DriverSignerName);

            Assert.AreEqual("oem18.inf", entries[1].DriverPublishedName);
            Assert.AreEqual("Intel", entries[1].DriverPkgProvider);
            Assert.AreEqual("System devices", entries[1].DriverClass);
            Assert.AreEqual(new DateTime(2012, 12, 17, 0, 0, 0, DateTimeKind.Unspecified), entries[1].DriverDate);
            Assert.AreEqual(new Version(9, 0, 0, 1287), entries[1].DriverVersion);
            Assert.AreEqual("Microsoft Windows Hardware Compatibility Publisher", entries[1].DriverSignerName);
        }

        [TestMethod()]
        public void ParseChinesePnpUtilEnumerateResultTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(ChinesePnpUtilEnumerateOutput);

            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Count);

            Assert.AreEqual("oem0.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Microsoft", entries[0].DriverPkgProvider);
            Assert.AreEqual("打印机", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2006, 06, 21, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(6, 1, 7600, 16385), entries[0].DriverVersion);
            Assert.AreEqual("Microsoft Windows", entries[0].DriverSignerName);
        }

        [TestMethod()]
        public void ParseLongChinesePnpUtilEnumerateResultTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(LongChinesePnpUtilEnumerateOutput);

            Assert.IsNotNull(entries);
            Assert.AreEqual(95, entries.Count);
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverPublishedName)));
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverPkgProvider)));
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverClass)));
            Assert.IsTrue(entries.All(e => e.DriverDate != default(DateTime)));
            Assert.IsTrue(entries.All(e => e.DriverVersion != null));

            // DriverSignerName is not always available in this sample
            // Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverSignerName)));
        }

        [TestMethod()]
        public void ParseRussianPnpUtilEnumerateResultTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(RussianPnpUtilEnumerateOutput);

            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Count);

            Assert.AreEqual("oem0.inf", entries[0].DriverPublishedName);
            Assert.AreEqual("Cisco Systems", entries[0].DriverPkgProvider);
            Assert.AreEqual("Сетевые адаптеры", entries[0].DriverClass);
            Assert.AreEqual(new DateTime(2014, 02, 26, 0, 0, 0, DateTimeKind.Unspecified), entries[0].DriverDate);
            Assert.AreEqual(new Version(3, 1, 6019, 0), entries[0].DriverVersion);
            Assert.AreEqual("Microsoft Windows Hardware Compatibility Publisher", entries[0].DriverSignerName);
        }

        [TestMethod()]
        public void ParseLongRussianPnpUtilEnumerateResultTest()
        {
            List<DriverStoreEntry> entries = PnpUtil.ParsePnpUtilEnumerateResult(LongRussianPnpUtilEnumerateOutput);

            Assert.IsNotNull(entries);
            Assert.AreEqual(38, entries.Count);
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverPublishedName)));
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverPkgProvider)));
            Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverClass)));
            Assert.IsTrue(entries.All(e => e.DriverDate != default(DateTime)));
            Assert.IsTrue(entries.All(e => e.DriverVersion != null));

            // DriverSignerName is not always available in this sample
            // Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.DriverSignerName)));
        }
    }
}