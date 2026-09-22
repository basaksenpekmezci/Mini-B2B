# Mini B2B E-Ticaret Projesi

> **Durum:** Gün 1 + Gün 2 tamamlandı (proje iskeleti, veritabanı şeması, auth, admin ürün/kullanıcı/sipariş
> yönetimi, mağaza tarafında dinamik grid, sepet ve sipariş oluşturma, "Siparişlerim").

## Kullanılan Teknolojiler

- **.NET 8** (ASP.NET Core MVC, Razor Views)
- **SQL Server** (LocalDB / Express / Developer — herhangi biri)
- **Dapper** — ADO.NET üzerine ince bir katman; ORM yerine elle yazılmış parametreli SQL kullanılıyor
- **ASP.NET Core Cookie Authentication** — kendi yazdığımız kullanıcı/rol modeli ile (Identity paketi kullanılmadı)
- **PBKDF2 (Rfc2898DeriveBytes)** — şifre hashleme
- **Bootstrap 5** (CDN üzerinden) — arayüz

Mimari: Tek web projesi içinde katmanlı yapı — `Domain` (entity'ler), `Data/Repositories` (Dapper ile SQL erişimi), `Services` (iş kuralları/validasyon), `Controllers` + `Areas/Admin` (MVC).

## Kurulum

### 1. Gereksinimler
- .NET 8 SDK
- SQL Server (LocalDB Visual Studio ile birlikte gelir; yoksa SQL Server Express indirilebilir)

### 2. Veritabanını Oluşturma
`db/` klasöründeki script'leri sırayla çalıştırın:

```
sqlcmd -S (localdb)\MSSQLLocalDB -i db/01_schema.sql
sqlcmd -S (localdb)\MSSQLLocalDB -i db/02_seed.sql
```

(SSMS kullanıyorsanız dosyaları açıp sırayla F5 ile çalıştırmanız yeterli.)

Bu script'ler `MiniB2B` veritabanını oluşturur, tüm tabloları (Users, Categories, Products, ProductGridColumns, Cart, CartItems, Orders, OrderItems) kurar ve örnek ürün/kategori/grid-kolon verisiyle doldurur.

> **Docker (azure-sql-edge) + macOS ile ilgili not:** Homebrew'daki güncel `sqlcmd` (go-sqlcmd 1.x), Go'nun
> `crypto/x509` paketindeki katı sertifika ayrıştırması nedeniyle azure-sql-edge'in kendiliğinden ürettiği
> (negatif seri numaralı) self-signed sertifikayı reddedip `TLS Handshake failed: tls: failed to parse
> certificate from server: x509: negative serial number` hatası verebilir — bu `-C`/`-N o` gibi bayraklarla
> çözülmez, çünkü hata sertifika doğrulanmadan önce, ayrıştırma aşamasında oluşur. Bu ortamda script'ler
> `sqlcmd` yerine, projenin zaten referans verdiği `Microsoft.Data.SqlClient` (.NET'in kendi TLS yığınını
> kullanır ve bu sertifikayı sorunsuz kabul eder) ile küçük bir yardımcı konsol uygulaması üzerinden
> çalıştırıldı. Aynı sorunu yaşarsanız alternatif olarak: (a) Azure Data Studio / DBeaver gibi Go tabanlı
> olmayan bir istemci kullanın, (b) `Microsoft.Data.SqlClient` ile script'leri kendiniz çalıştırın, ya da
> (c) container'a gerçek (pozitif seri numaralı) bir sertifika mount edin.

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
  01_schema.sql        # tablolar, ilişkiler, kısıtlar
  02_seed.sql           # kategori/ürün/grid-kolon örnek verisi
src/MiniB2B.Web/
  Domain/               # entity sınıfları (Product, Category, Cart, Order, ProductGridColumn, User)
  Data/
    DapperContext.cs
    DbInitializer.cs     # admin kullanıcı seed
    Repositories/         # Dapper ile SQL erişimi (arayüz + implementasyon)
  Services/             # PasswordHasher, AuthService, ProductService, CartService, OrderService,
                        # ProductGridRenderer (dinamik grid reflection), OrderStatusBadge
  Models/               # MVC ViewModel'leri (Login/Register)
  Controllers/          # Account, Home (mağaza/grid), Cart, Orders (Siparişlerim)
  Areas/Admin/          # Yönetim paneli (Products, Users, Orders controller + view'lar)
  Views/                # Mağaza tarafı view'ları
  wwwroot/               # statik dosyalar (css, js)
```

## Şimdiye Kadar Tamamlanan Özellikler

### Gün 1
- [x] Veritabanı şeması (tüm tablolar, FK/PK/UNIQUE/CHECK kısıtları)
- [x] Kayıt ol / Giriş yap / Çıkış yap (cookie auth, PBKDF2 şifre hash)
- [x] Yetkisiz kullanıcıların admin alanına erişiminin engellenmesi (`[Authorize(Roles = "Admin")]`)
- [x] Admin: Ürün listeleme + arama (ürün adı/kodu/marka/üretici kodu/özel kodlar/açıklama — tüm metinsel alanlarda, SQL tarafında tek sorgu)
- [x] Admin: Ürün ekleme / düzenleme (backend validasyonu ile)
- [x] Admin: Kullanıcı listeleme, detay görüntüleme, düzenleme

### Gün 2
- [x] Mağaza: Ürün listeleme/arama + dinamik grid — kolonlar (`ColumnKey`, sıra, render tipi, hizalama,
      genişlik, cihaz görünürlüğü) `ProductGridColumns` tablosundan okunur; her hücrenin değeri
      `ProductGridRenderer.GetValue` ile reflection üzerinden çözülür, kod değiştirmeden yeni kolon
      eklenebilir. `ShowOnDesktop/Tablet/Mobile` bayrakları Bootstrap responsive sınıflarına eşlenir.
- [x] Stok durumu ham sayı yerine ürün bazlı `KritikStokSeviyesi`'ne göre Var/Kritik/Yok rozeti (`Product.StokDurumu`)
- [x] Ürün detay popup'ı (Bootstrap modal + `/Home/ProductDetails/{id}` partial view, AJAX ile yüklenir)
- [x] Sepet: ekleme (grid üzerinden adet girip doğrudan, ürün detayına gitmeden), adet güncelleme, ürün çıkarma
- [x] Sipariş oluşturma: tek SQL transaction içinde, her ürün için `UPDLOCK/ROWLOCK` ile stok kontrolü
      (yetersizse tüm işlem rollback + "... için yeterli stok bulunmamaktadır. Mevcut stok: N." mesajı),
      ürün/fiyat snapshot'ı (`OrderItems.UrunKodu/UrunAdi/BirimFiyat`), stok düşümü ve sepetin temizlenmesi
      aynı transaction'da yapılır (`OrderService.CreateOrderFromCartAsync`)
- [x] "Siparişlerim" — kullanıcının kendi siparişleri (liste + detay); sadece sipariş sahibi erişebilir
- [x] Admin: Sipariş listesi + detay + durum değiştirme (Onaylandı/Reddedildi); değişiklik kullanıcının
      "Siparişlerim" ekranına anında yansır (aynı tablo, ayrı okuma)

**Tasarım tercihleri:**
- Sepette aynı ürün tekrar eklenirse (`UQ_CartItems_Cart_Product` kısıtı gereği) mevcut satırın adedi
  artırılır — kullanıcı için tutarlı, öngörülebilir bir sepet davranışı.
- Sipariş numarası (`SiparisNo`) `SP` + UTC zaman damgası (milisaniyeye kadar) formatında üretilir; tek
  transaction içinde üretildiği için pratikte çakışma riski yoktur.
- Ürün kataloğu (Ana Sayfa) girişsiz de görüntülenebilir; sepete ekleme/sipariş/"Siparişlerim" gibi
  giriş gerektiren aksiyonlar `[Authorize]` ile korunur ve girişsiz kullanıcı login sayfasına yönlendirilir.
