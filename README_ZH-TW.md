<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_KO.md">한국어</a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md"><b>繁體中文</b></a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ 警告
**Driver Store Explorer 會修改 Windows 驅動程式存放區。不當使用可能導致系統故障、Windows 無法啟動或裝置功能喪失。請在操作前充分瞭解風險。在刪除任何驅動程式之前，請務必進行備份。**

---

## 什麼是 Driver Store Explorer？

Driver Store Explorer (RAPR) 是一款功能強大的工具，用於檢視、管理和清理 Windows [驅動程式存放區](https://msdn.microsoft.com/en-us/library/ff544868(VS.85).aspx)。它面向進階使用者和系統管理員。如果您不熟悉 Windows 內部機制，不建議使用此工具。

---


## 功能

### 核心操作
* **瀏覽和列表**：檢視所有第三方驅動程式套件的詳細中繼資料（大小、版本、日期等）
* **新增/安裝**：安裝新的驅動程式套件，支援可選的自動裝置安裝
* **移除/刪除**：刪除單一或多個驅動程式，支援對正在使用的驅動程式進行強制刪除
	_注意：移除或刪除驅動程式的確切影響取決於系統狀態和使用情況。請謹慎操作，因為移除可能影響裝置功能或需要為某些硬體重新安裝驅動程式。_
* **匯出**：將選定或所有驅動程式備份到有組織的資料夾結構中
* **智慧清理**：自動識別並選取舊的/未使用的驅動程式版本

### 進階功能
* **多種 API**：支援原生 Windows API、DISM 或 PnPUtil 後端，具備自動偵測功能
* **線上和離線**：支援本機電腦或離線 Windows 映像驅動程式存放區
* **裝置關聯**：檢視已連線裝置及其存在狀態
* **批次操作**：支援多選批次操作，附帶進度追蹤
* **即時搜尋**：即時篩選和 CSV 匯出，便於分析

### 使用者介面
* **多語言**：支援 20 多種語言，包括從右到左的文字
* **可自訂**：可排序的欄位、群組和彈性的版面配置
* **視覺回饋**：色彩編碼、選取醒目提示和詳細日誌記錄

---

## 瞭解驅動程式狀態和移除選項

- **舊驅動程式：** 當系統中存在較新版本時，驅動程式被視為「舊」驅動程式。移除這些驅動程式可以幫助釋放空間並減少雜亂，但可能會影響與某些裝置或組態的相容性。建議在移除前備份驅動程式。「選取舊驅動程式」功能可以自動識別舊驅動程式，但結果可能有所不同。
- **灰色裝置名稱：** 裝置名稱顯示為灰色的驅動程式與目前未連線的裝置（如相機、手機或外接硬碟）相關聯。如果移除這些驅動程式，將來重新連線裝置時需要重新安裝。
- **強制刪除：** 如果需要刪除正在使用的驅動程式，請使用此選項。注意：此選項可能不適用於列印驅動程式。
---

## 螢幕截圖
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## 安裝

### 系統需求
- Windows 7 或更新版本
- .NET Framework 4.6.2 或更新版本
- 系統管理員權限

### 方式一：下載預先建置版本（建議）
1. 前往[最新版本頁面](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. 下載最新的 ZIP 壓縮檔
3. 將檔案解壓縮到您選擇的資料夾中
4. 執行 `Rapr.exe`

### 方式二：透過 Winget 安裝（建議）
```powershell
winget install lostindark.DriverStoreExplorer
```
安裝後，執行以下命令啟動工具：
```powershell
rapr
```

### 方式三：從原始碼建置
1. 複製或下載此存放庫
2. 在 Visual Studio 2022 中開啟 `Rapr.sln`
3. 建置方案（建置 → 建置方案 或 Ctrl+Shift+B）
4. 從輸出目錄執行可執行檔

---

## 專案歷史
最初託管於 [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer)。

## 致謝
- [ObjectListView](http://objectlistview.sourceforge.net/)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## 贊助商
Windows 上的免費程式碼簽署由 [SignPath.io] 提供，憑證由 [SignPath Foundation] 提供。

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
