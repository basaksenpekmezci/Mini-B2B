# Mini B2B E-Ticaret Projesi

Temel B2B e-ticaret süreçlerini uçtan uca kapsayan bir web uygulaması: ürün/kullanıcı/sipariş yönetimi
içeren bir **Yönetim Paneli** ve dinamik ürün grid'i, sepet ve sipariş akışı içeren bir **Kullanıcı
Arayüzü**.

## Kullanılan Teknolojiler

- **.NET 8** (ASP.NET Core MVC, Razor Views)
- **SQL Server** (LocalDB / Express / Developer / Docker — herhangi biri)
- **Dapper** — ORM yerine elle yazılmış parametreli SQL kullanılıyor (bkz. [Mimari ve Teknik Tercihler](#mimari-ve-teknik-tercihler))
- **ASP.NET Core Cookie Authentication** — kendi yazdığımız kullanıcı/rol modeli ile (Identity paketi kullanılmadı)
- **PBKDF2 (Rfc2898DeriveBytes)** — şifre hashleme
- **Bootstrap 5** (CDN üzerinden) — arayüz

## Kurulum

### 1. Gereksinimler
- .NET 8 SDK
- SQL Server (LocalDB Visual Studio ile birlikte gelir; yoksa SQL Server Express veya Docker image kullanılabilir)

### 2. Veritabanını Oluşturma
`db/` klasöründeki script'leri sırayla çalıştırın:

```
sqlcmd -S (localdb)\MSSQLLocalDB -i db/01_schema.sql
sqlcmd -S (localdb)\MSSQLLocalDB -i db/02_seed.sql
```

(SSMS kullanıyorsanız dosyaları açıp sırayla F5 ile çalıştırmanız yeterli.)

Bu iki script `MiniB2B` veritabanını oluşturur, tüm tabloları (Users, Categories, Products,
ProductGridColumns, Cart, CartItems, Orders, OrderItems) ve `SiparisNoSequence` nesnesini kurar,
örnek ürün/kategori/grid-kolon verisiyle doldurur. **Bu iki script'in taze bir kurulumda tek başına
yeterlidir** — üçüncü bir script'e gerek yoktur.

**Zaten bu projeyle daha önce (03 numaralı script eklenmeden önce) kurulmuş bir veritabanınız varsa**,
sadece eksik parçayı eklemek için ek olarak şunu çalıştırın (idempotent'tir, veriyi silmez):

```
sqlcmd -S (localdb)\MSSQLLocalDB -i db/03_add_siparisno_sequence.sql
```

### 3. Bağlantı Ayarı
`src/MiniB2B.Web/appsettings.json` içindeki `ConnectionStrings:DefaultConnection`, Windows Authentication kullanan LocalDB için güvenli (şifre içermeyen) bir varsayılan değer içerir:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MiniB2B;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

SQL Server Express/Developer kullanıyorsanız `Server=` kısmını örn. `Server=localhost\\SQLEXPRESS` olarak değiştirin.

**Şifre içeren bir bağlantı dizesi (Docker/`sa`, SQL Server Authentication vb.) kullanıyorsanız bunu
appsettings.json içine yazmayın** — proje `MiniB2B.Web.csproj` içinde bir `UserSecretsId` ile zaten
[.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) için hazırlanmıştır;
`ASPNETCORE_ENVIRONMENT=Development` iken appsettings.json'daki değeri otomatik olarak (ve appsettings.json'ı
ezerek) override eder. Repo dışında, `~/.microsoft/usersecrets/` altında saklanır, asla git'e girmez:

```
cd src/MiniB2B.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MiniB2B;User Id=sa;Password=<sa-şifreniz>;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true"
```

`Properties/launchSettings.json` (repoda mevcut) `dotnet run`'ı otomatik olarak `Development` ortamında
başlatır, bu sayede yukarıdaki komutla ayarlanan secret ekstra bir yapılandırma gerekmeden devreye girer.

### 4. Çalıştırma

```
cd src/MiniB2B.Web
dotnet restore
dotnet run
```

Uygulama ilk açılışta, veritabanında `admin` kullanıcı adı yoksa otomatik olarak bir yönetici hesabı oluşturur (bkz. aşağıdaki varsayılan kullanıcı bilgileri). Bu, şifre hash'inin uygulama kodu içinde (SQL script ile değil) üretilmesi gerektiği için böyle tasarlandı.

Tarayıcıda `http://localhost:5080` (veya konsolda gösterilen port) adresine gidin.

## Varsayılan Kullanıcılar

| Rol | Kullanıcı Adı | Şifre | Not |
|---|---|---|---|
| Admin | `admin` | `Admin123!` | İlk çalıştırmada otomatik oluşturulur |
| Müşteri | — | — | `/Account/Register` üzerinden kayıt olun |

## Proje Yapısı

```
MiniB2B.sln
db/
  01_schema.sql                     # tablolar, ilişkiler, kısıtlar, SiparisNoSequence
  02_seed.sql                       # kategori/ürün/grid-kolon örnek verisi
  03_add_siparisno_sequence.sql     # ALTER: mevcut bir veritabanına SiparisNoSequence eklemek için
src/MiniB2B.Web/
  Domain/               # entity sınıfları (Product, Category, Cart, Order, ProductGridColumn, User)
  Data/
    DapperContext.cs
    DbInitializer.cs     # admin kullanıcı seed
    PagedResult.cs        # sunucu taraflı sayfalama sonucu (Items + TotalCount)
    Repositories/         # Dapper ile SQL erişimi (arayüz + implementasyon)
  Services/             # PasswordHasher, AuthService, ProductService, CartService, OrderService,
                        # ProductGridRenderer (dinamik grid reflection), OrderStatusBadge
  Models/               # MVC ViewModel'leri (Login/Register)
  Controllers/          # Account, Home (mağaza/grid), Cart, Orders (Siparişlerim)
  Areas/Admin/          # Yönetim paneli (Products, Users, Orders controller + view'lar)
  Views/                # Mağaza tarafı view'ları
  wwwroot/               # statik dosyalar (css, js)
```

## Özellikler

### Yönetim Paneli
- **Ürün yönetimi:** listeleme + arama (ürün adı/kodu/marka/üretici kodu/özel kodlar/açıklama — tüm
  metinsel alanlarda, SQL tarafında tek sorgu), kategori ve marka filtresi, sunucu taraflı sayfalama,
  ürün ekleme/düzenleme (backend validasyonu), ürünü pasife alma/tekrar aktif etme (Aktif/Pasif/Tümü
  filtresi ile).
- **Kullanıcı yönetimi:** listeleme, detay görüntüleme, düzenleme; şifreler PBKDF2 ile hash'lenip
  saklanır (asla açık metin değil).
- **Sipariş yönetimi:** sipariş listesi (no, kullanıcı, tarih, tutar, durum) + detay (kalemler, sipariş
  anındaki ürün/fiyat bilgisi); durum sadece "Beklemede" iken Onaylandı/Reddedildi yapılabilir, zaten
  sonuçlanmış siparişte anlaşılır hata döner; Reddedildi seçilirse stok otomatik iade edilir.

### Kullanıcı Arayüzü
- Ana sayfada kampanya/slider alanı ve ürün grid'i.
- Kayıt ol / giriş yap / çıkış yap (cookie auth); giriş gerektiren aksiyonlarda girişsiz kullanıcı
  login sayfasına yönlendirilir.
- **Ürün arama ve dinamik grid:** kolonlar (hangi alan, sırası, render tipi, hizalama, genişlik, hangi
  cihazda görünür) `ProductGridColumns` tablosundan okunur; kod değiştirmeden yeni kolon eklenebilir
  (bkz. [Mimari ve Teknik Tercihler](#mimari-ve-teknik-tercihler)). Kategori/marka filtresi ve sayfalama.
- Stok durumu ham sayı yerine ürün bazlı kritik seviyeye göre Var (yeşil) / Kritik (sarı) / Yok
  (kırmızı) rozeti ile gösterilir.
- Ürün detayı popup (modal) olarak açılır; grid üzerinden de doğrudan adet girip sepete eklenebilir.
- **Sepet:** ürün ekleme, adet güncelleme, çıkarma; sepet toplamı.
- **Sipariş oluşturma:** sepetteki ürünlerin stok yeterliliği backend'de kontrol edilir; yetersizse
  sipariş oluşturulmaz ve kullanıcıya hangi üründe ne kadar stok olduğu söylenir.
- **Siparişlerim:** kullanıcının kendi siparişleri (liste + detay); admin durumu değiştirdiğinde
  (Onaylandı/Reddedildi) bu ekrana anında yansır.

### Sipariş ve Stok Yönetimi (Backend İş Kuralları)
- Sipariş oluşturma tek SQL transaction içinde çalışır: sepet kalemleri + ürünler `WITH (UPDLOCK,
  ROWLOCK)` ile kilitlenerek tek sorguda okunur (fiyat/stok snapshot'ı bu kilitli okumadan gelir),
  pasif ürün varsa ya da stok yetersizse rollback yapılır.
- Sipariş kalemlerinde ürün kodu/adı/birim fiyat **sipariş anındaki** değerleriyle saklanır (snapshot);
  ürünün fiyatı sonradan değişse bile geçmiş sipariş etkilenmez.
- Sipariş durumu sadece "Beklemede" iken değiştirilebilir; "Reddedildi" olursa kalemlerdeki adetler
  tek bir set-based `UPDATE ... JOIN` ile ürünlerin stoğuna geri eklenir — aynı transaction içinde.
- Sipariş numarası SQL Server `SEQUENCE` nesnesiyle (`SP` + 6 haneli sıra, örn. `SP000001`) üretilir;
  eşzamanlı sipariş oluşturmalarda çakışma riski yoktur.
- Olmayan/pasif bir ürün sepete eklenemez veya sipariş edilemez.

### Veritabanı / SQL
- İlişkisel şema: tüm tablolarda PK/FK/UNIQUE/CHECK kısıtları ve uygun veri tipleri (bkz. `db/01_schema.sql`).
- Ürün arama/filtreleme/listeleme ve sayfalama tamamen SQL tarafında (`WHERE` + `OFFSET/FETCH`);
  sayfa kayıtları ve toplam kayıt sayısı tek round-trip'te (`QueryMultiple`) gelir — tüm kayıtlar
  uygulamaya çekilip filtrelenmez.
- Stok iadesi gibi çoklu satır güncellemeleri döngü yerine tek set-based `UPDATE ... JOIN` ile yapılır.
- Kullanıcıya ait siparişler, sepet ürünleri, sipariş detayları — hepsi ilişkiler üzerinden (FK join)
  tek sorgularla getirilir.

## Mimari ve Teknik Tercihler

**Katmanlı yapı:** Tek web projesi içinde `Domain` (entity'ler) → `Data/Repositories` (Dapper ile SQL
erişimi, arayüz + implementasyon) → `Services` (iş kuralları/validasyon, transaction yönetimi) →
`Controllers` + `Areas/Admin` (MVC, HTTP katmanı). Her katman bir öncekini soyutlar; controller'lar
SQL bilmez, repository'ler iş kuralı bilmez.

**Neden Dapper (EF Core değil):** Ödevin değerlendirme kriterlerinden biri doğrudan SQL bilgisi. Dapper,
ORM'in üretmediği elle yazılmış, optimize edilmiş SQL (OFFSET/FETCH sayfalama, UPDLOCK/ROWLOCK ile
transaction içi kilitleme, set-based UPDATE...JOIN, QueryMultiple ile çoklu sonuç kümesi) yazmayı ve
bunun görünür/incelenebilir olmasını sağlıyor.

**Transaction + UPDLOCK ile stok kontrolü:** Sipariş oluşturma sırasında sepetteki her ürünün stok
satırı `WITH (UPDLOCK, ROWLOCK)` ile okunur ve aynı transaction commit'lenene kadar kilitli tutulur.
Bu, iki kullanıcının aynı anda aynı üründen (örn. tek kalan) stok görüp ikisinin de siparişi
oluşturabilmesi gibi bir race condition'ı engeller — kilit sayesinde ikinci istek birincinin
commit/rollback'ini bekler ve güncel stok değerini görür.

**Fiyat/ürün snapshot'ı:** `OrderItems` tablosu `UrunKodu`, `UrunAdi`, `BirimFiyat` alanlarını kendi
içinde saklar (Products tablosuna sadece FK ile değil). Böylece ürünün fiyatı ileride değişse veya
ürün pasife alınsa bile, geçmiş bir sipariş o anki bilgileriyle doğru şekilde görüntülenmeye devam eder.

**Reddedilen siparişte stok iadesi:** Admin bir siparişi reddettiğinde, o siparişin tüm kalemlerindeki
adetler tek bir set-based sorguyla (`UPDATE Products p JOIN OrderItems oi ...`) ilgili ürünlerin
stoğuna geri eklenir — kalem sayısı ne olursa olsun tek sorgu, ve durum güncellemesiyle aynı
transaction içinde (biri başarısız olursa diğeri de geri alınır).

**SEQUENCE ile sipariş no:** İlk halinde sipariş numarası zaman damgasından üretiliyordu; aynı
milisaniyede oluşan siparişlerde teorik çakışma riski taşıyordu. SQL Server'ın `SEQUENCE` nesnesi
transaction'lar arası kilitlenmeden atomik artış garantisi verdiği için hem çakışmaya kapalı hem
performanslı bir çözüm.

**Dinamik grid'in çalışma mantığı:** `ProductGridColumns` tablosu her kolon için `ColumnKey` (Product
sınıfındaki property adı), `OrderIndex`, `RenderType` (Text/Image/Currency/StockBadge/QtyInput),
hizalama/genişlik ve cihaz görünürlüğü (`ShowOnDesktop/Tablet/Mobile`) tutar. `ProductGridRenderer.GetValue`,
verilen `ColumnKey` için `typeof(Product).GetProperty(...)` ile reflection üzerinden değeri okur (sonuçlar
`ConcurrentDictionary` ile cache'lenir, ürün sayısı arttıkça reflection maliyeti tekrarlanmaz). View,
her kolon için `RenderType`'a göre farklı bir render stratejisi seçer (örn. `StockBadge` → renkli rozet,
`QtyInput` → adet girişi + sepete ekle formu — bu son ikisi Product üzerinde gerçek bir property değil,
özel render tipleri). Sonuç: yeni bir kolon eklemek veya sırasını/görünürlüğünü değiştirmek için kod
değişikliği gerekmez, sadece `ProductGridColumns` tablosunda satır eklenir/güncellenir.

## Örnek Kullanım Senaryosu

Uygulamayı test etmek için önerilen uçtan uca akış:

1. **Kayıt ol:** `/Account/Register` üzerinden yeni bir müşteri hesabı oluşturun (otomatik giriş yapılır).
2. **Sepete ekle:** Ana sayfada ürün grid'inden bir ürüne adet girip "Sepete Ekle"ye basın; "Detay"
   butonuyla popup'ta ürün bilgilerini de inceleyebilirsiniz.
3. **Stok yetersiz senaryosunu deneyin:** Stoğu düşük bir üründen (grid'de "Kritik" rozetli, örn.
   stok=2) mevcut stoktan fazla adet girip sepete ekleyin, `/Cart` üzerinden "Sipariş Oluştur"a basın —
   "... için yeterli stok bulunmamaktadır. Mevcut stok: N." mesajını görmelisiniz; sipariş oluşmaz.
4. **Sipariş verin:** Adedi stok sınırları içinde bir ürünle tekrar deneyin — sipariş oluşur, sepet
   temizlenir, `/Orders` (Siparişlerim) sayfasında yeni siparişi (`SP0000xx` numarasıyla, "Beklemede"
   durumunda) görürsünüz.
5. **Admin onaylasın/reddetsin:** `admin` / `Admin123!` ile giriş yapıp `/Admin/Orders`'a gidin,
   siparişin detayına girip "Onayla" veya "Reddet" deyin (Reddet seçilirse ürünün stoğunun arttığını
   admin ürün listesinden doğrulayabilirsiniz).
6. **Siparişlerim'de görün:** Müşteri hesabına dönüp `/Orders`'ı yenileyin — durumun (Onaylandı/Reddedildi)
   anında yansıdığını görürsünüz.

## Ek Not

**Docker (azure-sql-edge) + macOS:** Homebrew'daki güncel `sqlcmd` (go-sqlcmd), Go'nun katı sertifika
ayrıştırması yüzünden azure-sql-edge'in ürettiği self-signed sertifikayı reddedebilir (`x509: negative
serial number`) — bu `-C`/`-N o` ile çözülmez. Alternatif: script'leri Azure Data Studio/DBeaver gibi
başka bir istemciyle ya da `Microsoft.Data.SqlClient` (.NET'in kendi TLS yığınını kullanır, bu
sertifikayı sorunsuz kabul eder) ile çalıştırın.
