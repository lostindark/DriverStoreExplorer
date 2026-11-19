<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_ES.md">Español</a> | <a href="/README_FR.md">Français</a> | <a href="/README_TR.md">Türkçe</a> | <a href="/README_KO.md">한국어</a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md">繁體中文</a> | <a href="/README_JA.md"><b>日本語</b></a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ 警告
**Driver Store ExplorerはWindowsのドライバーストアを変更します。不適切に使用すると、システムの誤動作、Windowsの起動不能、またはデバイス機能の喪失を引き起こす可能性があります。続行する前にリスクを十分に理解してください。何かを削除する前には、必ずドライバーをバックアップしてください。**

---

## Driver Store Explorerとは？

Driver Store Explorer (RAPR)は、Windowsの[ドライバーストア (DriverStore)](https://learn.microsoft.com/windows-hardware/drivers/install/driver-store)を閲覧、管理、クリーンアップするための強力なツールです。上級ユーザーおよび管理者を対象としています。Windowsの内部構造に詳しくない場合は、本ツールの使用は推奨されません。

---


## 主な機能

### 基本操作
* **閲覧・一覧表示**: すべてのサードパーティ製ドライバーパッケージを詳細なメタデータ（サイズ、バージョン、日付など）とともに表示します
* **追加・インストール**: 新しいドライバーパッケージを追加・インストールします（オプションでデバイスの自動インストールにも対応）
* **削除**: 単一または複数のドライバーを削除します（使用中のドライバーに対する強制削除にも対応）  
	_注: ドライバーの削除による具体的な影響は、システムの状態や使用状況によって異なります。削除するとデバイスの機能に影響を与えたり、特定のハードウェアでドライバーの再インストールが必要になったりする場合があるため、十分にご注意ください。_
* **エクスポート**: 選択したドライバー、またはすべてのドライバーを整理されたフォルダー構造でバックアップします
* **スマートクリーンアップ**: 古いバージョンのドライバーや未使用のドライバーを自動的に識別して選択します

### 高度な機能
* **複数のAPIに対応**: ネイティブWindows API、DISM、またはPnPUtilバックエンドをサポートし、自動検出を行います
* **オンラインおよびオフライン対応**: ローカルマシンおよびオフラインのWindowsイメージのドライバーストアを操作できます
* **デバイスの関連付け**: 接続されているデバイスとその接続状態を表示します
* **一括操作**: 複数選択によるバッチ処理と進捗状況の追跡に対応しています
* **リアルタイム検索**: ライブフィルタリングおよび分析用のCSVエクスポートに対応しています

### ユーザーインターフェース
* **多言語対応**: 右横書き（RTL）言語を含め、20以上の言語に対応しています
* **カスタマイズ可能**: 列の並べ替え、グループ化、柔軟なレイアウト変更が可能です
* **視覚的フィードバック**: 色分け表示、選択項目の強調表示、詳細なログ記録に対応しています

---

## ドライバーの状態と削除オプションについて

- **古いドライバー:** システム上に新しいバージョンが存在する場合、以前のドライバーは「古い（old）」とみなされます。これらを削除すると、空き容量を増やして不要なファイルを減らすことができますが、特定のデバイスや構成との互換性に影響を与える可能性があります。削除前にドライバーをバックアップすることをご検討ください。「古いドライバーを選択」を使用すると古いドライバーを自動的に識別できますが、結果は環境によって異なる場合があります。
- **未使用のドライバー:** 「未使用のドライバーを選択」は、削除しても安全であると証明されたドライバーではなく、関連付けられたデバイスが存在しないドライバーを選択します。重要なウイルス対策ソフト、ネットワークサービス、プリンター、その他のソフトウェアドライバーも選択される可能性があります。ボタンとコンテキストメニューのどちらから実行する場合も、選択を変更する前に警告が表示されます（デフォルトは「キャンセル」です）。この警告はデフォルトで有効になっており、**オプション > 未使用のドライバーを選択する前に警告する**からオフにできます。これらのドライバーのいずれかにチェックが入っている間は、操作結果の横に黄色のステータスリマインダーが表示され続けます。エラーが発生した場合も、リマインダーを隠さずに赤色で表示されたままになります。ドライバーパッケージのエクスポートに成功すると、バックアップされたドライバーについてはリマインダーが消去されますが、CSVエクスポートや失敗したバックアップでは消去されません。削除する前に、選択されたドライバーを確認してバックアップしてください。
- **起動に必要なドライバー:** Windowsの起動に必要となる場合があるドライバーです。**オプション > 起動に必要なドライバーを古いドライバーと未使用のドライバーに含める**をオンにしない限り、「古いドライバーを選択」および「未使用のドライバーを選択」ではこれらのドライバーはスキップされます。
- **検索中の選択操作:** 検索ボックスで一覧を絞り込んでいる場合、「すべて選択」、「選択を反転」、および「選択」は、現在表示されているドライバーのみを変更します。「古いドライバーを選択」および「未使用のドライバーを選択」はドライバーストア全体を参照するため、まず検索をクリアし、選択したすべての項目が表示されたままになります。
- **グレー表示のデバイス名:** デバイス名がグレーで表示されているドライバーは、現在接続されていないデバイス（カメラ、スマートフォン、外付けドライブなど）に関連付けられています。これらのドライバーを削除した場合、将来そのデバイスを再接続した際にドライバーを再インストールする必要があります。
- **オフラインでのクリーンアップ:** オフラインのストアでは、デバイスの使用状況を確実に特定できないため、古いドライバーや未使用ドライバーの自動選択は無効になっています。削除対象として手動でドライバーを選択することは可能です。
- **強制削除:** 現在使用中のドライバーを削除する必要がある場合にこのオプションを使用します。注: このオプションはプリンタードライバーに対しては機能しない場合があります。
---

## スクリーンショット
![DriverStoreExplorerのスクリーンショット](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## インストール

### 動作要件
- Windows 7以降
- .NET Framework 4.7.2以降
- 管理者権限

### 方法1: ビルド済みバイナリをダウンロードする（推奨）
1. [最新リリースページ](https://github.com/lostindark/DriverStoreExplorer/releases/latest)にアクセスします
2. 最新のZIPアーカイブをダウンロードします
3. 任意のフォルダーにファイルを展開します
4. `Rapr.exe`を実行します

### 方法2: Wingetでインストールする（推奨）
```powershell
winget install lostindark.DriverStoreExplorer
```
インストール後、次のコマンドを実行してツールを起動します:
```powershell
rapr
```

### 方法3: Scoopでインストールする
```powershell
scoop bucket add extras
scoop install driverstoreexplorer
```
インストール後、次のコマンドを実行してツールを起動します:
```powershell
rapr
```

### 方法4: ソースコードからビルドする
1. このリポジトリをクローンまたはダウンロードします
2. Visual Studio 2022でRapr.slnを開きます
3. ソリューションをビルドします（ビルド → ソリューションのビルド、またはCtrl+Shift+B）
4. 出力ディレクトリから実行ファイルを実行します

---

## プロジェクトの沿革
元々は[https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer)でホストされていました。

## クレジット
- [ObjectListView](https://github.com/lostindark/ObjectListView)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## スポンサー
Windows向けの無償コード署名は[SignPath.io]、証明書は[SignPath Foundation]によって提供されています。

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
