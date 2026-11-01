<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_ES.md">Español</a> | <a href="/README_TR.md"><b>Türkçe</b></a> | <a href="/README_KO.md">한국어</a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md">繁體中文</a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ Uyarı
**Driver Store Explorer, Windows sürücü deposunu değiştirir. Yanlış kullanım sistem arızasına, Windows'un önyükleme yapamamasına veya aygıt işlevselliğinin kaybına yol açabilir. Devam etmeden önce riskleri bilin. Bir şeyi silmeden önce sürücülerinizi her zaman yedekleyin.**

---

## Driver Store Explorer nedir?

Driver Store Explorer (RAPR), Windows [DriverStore](https://learn.microsoft.com/tr-tr/windows-hardware/drivers/install/driver-store) içeriğini görüntülemek, yönetmek ve temizlemek için kullanılan güçlü bir araçtır. İleri düzey kullanıcılar ve yöneticiler için tasarlanmıştır. Windows'un iç işleyişine aşina değilseniz, bu aracı kullanmanız önerilmez.

---

## Özellikler

### Temel İşlemler
* **Gözat ve Listele**: Ayrıntılı meta verilerle (boyut, sürüm, tarih vb.) birlikte tüm üçüncü taraf sürücü paketlerini görüntüleyin
* **Ekle/Yükle**: İsteğe bağlı otomatik aygıt yüklemesiyle yeni sürücü paketleri yükleyin
* **Kaldır/Sil**: Kullanımda olan sürücüler için zorla silme desteğiyle tek veya birden fazla sürücüyü silin  
	_Not: Sürücüleri kaldırmanın veya silmenin tam etkisi sistem durumuna ve kullanım şekline bağlıdır. Kaldırma işlemi aygıt işlevselliğini etkileyebilir veya bazı donanımlar için sürücülerin yeniden yüklenmesini gerektirebilir; bu nedenle dikkatli olun._
* **Dışa Aktar**: Seçili sürücüleri veya tüm sürücüleri düzenli klasör yapıları halinde yedekleyin
* **Akıllı Temizleme**: Eski veya kullanılmayan sürücü sürümlerini otomatik olarak belirleyip seçin

### Gelişmiş Yetenekler
* **Birden Fazla API**: Yerel Windows API, DISM veya PnPUtil arka uç desteği ve otomatik algılama
* **Çevrimiçi ve Çevrimdışı**: Yerel makinede veya çevrimdışı Windows imaj sürücü deposunda çalışma
* **Aygıt Eşleştirme**: Bağlı aygıtları ve mevcut durumlarını görüntüleme
* **Toplu İşlemler**: İlerleme takibiyle birlikte toplu işlemler için çoklu seçim
* **Gerçek Zamanlı Arama**: Analiz için canlı filtreleme ve CSV dışa aktarma

### Kullanıcı Arayüzü
* **Çok Dilli**: RTL desteğiyle 20'den fazla dil
* **Özelleştirilebilir**: Sıralanabilir sütunlar, gruplama ve esnek düzen
* **Görsel Geri Bildirim**: Renk kodlaması, seçim vurgulama ve ayrıntılı günlükleme

---

## Sürücü Durumu ve Kaldırma Seçeneklerini Anlama

- **Eski sürücüler:** Sistem üzerinde daha yeni sürümler bulunduğunda sürücüler "eski" olarak kabul edilir. Bunları kaldırmak alan açmaya ve karmaşayı azaltmaya yardımcı olabilir, ancak bazı aygıtlar veya yapılandırmalarla uyumluluğu etkileyebilir. Kaldırmadan önce sürücüleri yedeklemeyi düşünün. "Eski Sürücüleri Seç" seçeneği eski sürücüleri otomatik olarak belirleyebilir, ancak sonuçlar değişebilir.
- **Kullanılmayan sürücüler:** "Kullanılmayan Sürücüleri Seç", silinmesinin güvenli olduğu kanıtlanmış sürücüleri değil, ilişkili bir aygıtı olmayan sürücüleri seçer. Antivirüs, ağ hizmeti, yazıcı ve diğer yazılımlar için gerekli sürücüler de seçilebilir. Hem düğme hem de bağlam menüsü, seçimi değiştirmeden önce bir uyarı gösterir; varsayılan seçenek İptal'dir. Bu uyarı varsayılan olarak etkindir ve **Seçenekler > Kullanılmayan Sürücüleri Seçmeden Önce Uyar** ile kapatılabilir. Bu sürücülerden biri işaretli kaldığı sürece durum çubuğunda sarı bir hatırlatma görünür; hatalar kırmızı gösterilir ama hatırlatmayı gizlemez. Sürücü paketlerini başarıyla dışa aktardığınızda, yedeklenen sürücüler için hatırlatma kalkar; CSV dışa aktarma ve başarısız yedeklemeler hatırlatmayı kaldırmaz. Silmeden önce seçilen sürücüleri gözden geçirin ve yedekleyin.
- **Önyükleme için kritik sürücüler:** Windows'un başlaması için bu sürücüler gerekebilir. **Seçenekler > Eski ve Kullanılmayan Sürücülerde Önyükleme Kritik Olanları Dahil Et** seçeneğini açmadığınız sürece "Eski Sürücüleri Seç" ve "Kullanılmayan Sürücüleri Seç" bu sürücüleri atlar.
- **Arama sırasında seçim:** Arama kutusu listeyi filtrelediğinde "Tümünü Seç", "Ters Seçim" ve "Seç" yalnızca gördüğünüz sürücüleri değiştirir. "Eski Sürücüleri Seç" ve "Kullanılmayan Sürücüleri Seç" tüm sürücü deposuna baktığı için önce aramayı temizler; böylece seçtikleri her şey görünür kalır.
- **Gri aygıt adları:** Aygıt adı gri renkte gösterilen sürücüler, şu anda bağlı olmayan aygıtlarla ilişkilidir (örneğin kameralar, telefonlar veya harici sürücüler). Bu sürücüleri kaldırırsanız, aygıtı gelecekte yeniden bağladığınızda bunları yeniden yüklemeniz gerekir.
- **Çevrimdışı temizleme:** Aygıt kullanımı güvenilir şekilde belirlenemediği için çevrimdışı depolarda eski ve kullanılmayan sürücülerin otomatik seçimi kapalıdır. Sürücüleri kaldırmak için yine de elle seçebilirsiniz.
- **Zorla silme:** Şu anda kullanımda olan bir sürücüyü silmeniz gerekiyorsa bu seçeneği kullanın. Not: Bu seçenek yazıcı sürücülerinde çalışmayabilir.
---

## Ekran Görüntüsü
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## Kurulum

### Gereksinimler
- Windows 7 veya daha yeni
- .NET Framework 4.7.2 veya daha yeni
- Yönetici yetkileri

### Seçenek 1: Hazır İkili Dosyayı İndir (Önerilen)
1. [en son sürüm sayfasına](https://github.com/lostindark/DriverStoreExplorer/releases/latest) gidin
2. En son ZIP arşivini indirin
3. Dosyaları istediğiniz bir klasöre çıkarın
4. `Rapr.exe` dosyasını çalıştırın

### Seçenek 2: Winget ile Kurulum (Önerilen)
```powershell
winget install lostindark.DriverStoreExplorer
```
Kurulumdan sonra aracı şu komutla başlatın:
```powershell
rapr
```

### Seçenek 3: Scoop ile Kurulum
```powershell
scoop bucket add extras
scoop install driverstoreexplorer
```
Kurulumdan sonra aracı şu komutla başlatın:
```powershell
rapr
```

### Seçenek 4: Kaynaktan Derleme
1. Bu depoyu klonlayın veya indirin
2. `Rapr.sln` dosyasını Visual Studio 2022'de açın
3. Çözümü derleyin (Build → Build Solution veya Ctrl+Shift+B)
4. Çıktı dizinindeki çalıştırılabilir dosyayı başlatın

---

## Proje Geçmişi
İlk olarak [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer) adresinde barındırılıyordu.

## Katkıda Bulunanlar
- [ObjectListView](https://github.com/lostindark/ObjectListView)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## Sponsorlar
Windows üzerinde ücretsiz kod imzalama [SignPath.io] tarafından sağlanır, sertifika ise [SignPath Foundation] tarafından sağlanır.

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
