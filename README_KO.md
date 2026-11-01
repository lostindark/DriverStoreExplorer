<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_ES.md">Español</a> | <a href="/README_TR.md">Türkçe</a> | <a href="/README_KO.md"><b>한국어</b></a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md">繁體中文</a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ 경고
**Driver Store Explorer는 Windows 드라이버 저장소를 수정합니다. 부적절하게 사용하면 시스템 오작동, Windows 부팅 불가 또는 장치 기능 손실이 발생할 수 있습니다. 진행하기 전에 위험을 숙지하세요. 드라이버를 삭제하기 전에 항상 백업하세요.**

---

## Driver Store Explorer란 무엇인가요?

Driver Store Explorer (RAPR)는 Windows [드라이버 저장소](https://learn.microsoft.com/ko-kr/windows-hardware/drivers/install/driver-store)를 보고, 관리하고, 정리하는 강력한 도구입니다. 고급 사용자와 관리자를 대상으로 합니다. 내부 구조에 익숙하지 않은 경우 이 도구를 사용하지 않는 것이 좋습니다.

---


## 기능

### 핵심 운영
* **찾아보기 및 목록**: 자세한 메타데이터 (크기, 버전, 날짜 등)와 함께 모든 타사 드라이버 패키지를 확인합니다
* **추가/설치**: 자동 장치 설치 (선택 사항)를 통해 새 드라이버 패키지를 설치합니다
* **제거/삭제**: 사용 중인 드라이버를 강제로 삭제하여 하나 또는 여러 드라이버를 삭제합니다  
	_참고: 드라이버를 제거하거나 삭제하는 것의 정확한 영향은 시스템 상태 및 사용 방식에 따라 달라집니다. 제거 시 장치 기능에 영향을 미치거나 특정 하드웨어의 드라이버를 다시 설치해야 할 수 있으므로 주의하시기 바랍니다._
* **내보내기**: 선택한 드라이버나 모든 드라이버를 정리된 폴더 구조로 백업합니다
* **스마트 정리**: 자동으로 오래되거나 사용되지 않는 드라이버 버전을 식별하고 선택합니다

### 고급 기능
* **다중 API**: 자동 감지 기능을 갖춘 네이티브 Windows API, DISM 또는 PnPUtil 백엔드 지원
* **온라인 및 오프라인**: 로컬 머신 또는 오프라인 Windows 이미지 드라이버 저장소를 통해 작업
* **장치 연결**: 연결된 장치 및 해당 상태 확인
* **일괄 작업**: 진행 상황 추적 기능을 갖춘 대량 작업을 위한 다중 선택
* **실시간 검색**: 분석을 위한 실시간 필터링 및 CSV 내보내기

### 사용자 인터페이스
* **다국어**: RTL을 지원하는 20개 이상의 언어
* **사용자 지정 가능**: 정렬 가능한 열, 그룹화 및 유연한 레이아웃
* **시각적 피드백**: 색상 코딩, 선택 영역 강조 표시 및 상세 로깅

---

## 드라이버 상태 및 제거 옵션 이해하기

- **오래된 드라이버:** 시스템에 최신 버전이 있는 경우 드라이버는 "오래된" 드라이버로 간주됩니다. 이러한 드라이버를 제거하면 공간을 확보하고 불필요한 요소를 줄이는 데 도움이 되지만 특정 장치 또는 구성과의 호환성에 영향을 줄 수 있습니다. 제거하기 전에 드라이버를 백업하는 것이 좋습니다. "오래된 드라이버 선택"을 사용하면 오래된 드라이버를 자동으로 식별할 수 있지만 결과는 다를 수 있습니다.
- **사용되지 않는 드라이버:** "사용되지 않는 드라이버 선택"은 안전하게 삭제할 수 있다고 확인된 드라이버가 아니라, 연결된 장치가 없는 드라이버를 선택합니다. 바이러스 백신, 네트워크 서비스, 프린터 및 기타 소프트웨어에 꼭 필요한 드라이버도 선택될 수 있습니다. 버튼과 상황에 맞는 메뉴 모두 선택을 바꾸기 전에 경고를 표시하며, 기본 선택은 취소입니다. 이 경고는 기본적으로 켜져 있으며 **옵션 > 사용하지 않는 드라이버 선택 전에 경고**에서 끌 수 있습니다. 이러한 드라이버가 하나라도 선택되어 있는 동안 상태 표시줄에 노란색 알림이 남아 있으며, 오류는 빨간색으로 표시되지만 알림을 가리지 않습니다. 드라이버 패키지를 성공적으로 내보내면 백업한 드라이버에 대한 알림이 사라지지만, CSV 내보내기와 실패한 백업은 알림을 지우지 않습니다. 삭제하기 전에 선택한 드라이버를 검토하고 백업하세요.
- **부팅 필수 드라이버:** Windows를 시작하는 데 이 드라이버가 필요할 수 있습니다. **옵션 > 오래된 드라이버 및 사용하지 않는 드라이버에 부팅 필수 항목 포함**을 켜지 않으면 "오래된 드라이버 선택"과 "사용되지 않는 드라이버 선택"은 이 드라이버를 건너뜁니다.
- **검색 중 선택:** 검색 상자로 목록을 필터링하면 "모두 선택", "선택 반전", "선택"은 화면에 보이는 드라이버만 변경합니다. "오래된 드라이버 선택"과 "사용되지 않는 드라이버 선택"은 전체 드라이버 저장소를 확인하므로 먼저 검색을 지우며, 선택한 드라이버는 모두 화면에 표시됩니다.
- **회색 장치 이름:** 장치 이름이 회색으로 표시된 드라이버는 현재 연결되지 않은 장치 (예: 카메라, 휴대폰 또는 외장 드라이브)와 관련된 드라이버입니다. 이러한 드라이버를 제거하면 나중에 장치를 다시 연결할 때 다시 설치해야 합니다.
- **오프라인 정리:** 오프라인 저장소에서는 장치 사용 여부를 확실하게 알 수 없으므로 오래되거나 사용되지 않는 드라이버의 자동 선택이 꺼져 있습니다. 제거할 드라이버는 직접 선택할 수 있습니다.
- **강제 삭제:** 현재 사용 중인 드라이버를 삭제해야 하는 경우 이 옵션을 사용하세요. 참고: 이 옵션은 인쇄 드라이버에는 작동하지 않을 수 있습니다.
---

## 스크린샷
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## 설치

### 요구 사항
- Windows 7 이상
- .NET Framework 4.7.2 이상
- 관리자 권한

### 옵션 1: 사전 빌드된 바이너리 다운로드 (권장)
1. [최신 릴리즈 페이지](https://github.com/lostindark/DriverStoreExplorer/releases/latest)로 가기
2. 최신 ZIP 파일을 다운로드
3. 원하는 폴더에 파일의 압축 풀기
4. `Rapr.exe` 실행

### 옵션 2: Winget을 통해 설치 (권장)
```powershell
winget install lostindark.DriverStoreExplorer
```
설치 후 다음을 실행하여 도구 실행:
```powershell
rapr
```

### 옵션 3: Scoop을 통해 설치
```powershell
scoop bucket add extras
scoop install driverstoreexplorer
```
설치 후 다음을 실행하여 도구 실행:
```powershell
rapr
```

### 옵션 4: 소스에서 빌드
1. 이 저장소 복제 또는 다운로드
2. Visual Studio 2022에서 `Rapr.sln` 열기
3. 솔루션 빌드 (빌드 → 솔루션 빌드 또는 Ctrl+Shift+B)
4. 출력 디렉터리에서 실행 파일 실행

---

## 프로젝트 기록
원래는 [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer) 에서 호스팅되었습니다.

## 크레딧
- [ObjectListView](https://github.com/lostindark/ObjectListView)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## 스폰서
Windows에서 무료 코드 서명은 [SignPath.io]에서 제공하고, 인증서는 [SignPath Foundation]에서 제공합니다.

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org

## 번역
한국어: [비너스걸💗](https://github.com/VenusGirl)
