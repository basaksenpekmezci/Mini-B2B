/*
    Seed Data
    Not: PasswordHash/PasswordSalt değerleri burada SQL içinde üretilmiyor;
    uygulama ilk çalıştığında Program.cs > DbInitializer bu kullanıcıları
    doğru PBKDF2 hash ile otomatik oluşturur (bkz. README - Varsayılan Kullanıcılar).
    Bu script sadece ürün/kategori/grid-config seed'ini içerir.
*/

USE MiniB2B;
GO

-- Kategoriler
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Ad) VALUES
    (N'Elektronik'),
    (N'Ofis Malzemeleri'),
    (N'Endüstriyel Ekipman'),
    (N'Gıda'),
    (N'Temizlik Ürünleri');
END
GO

-- Ürünler
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @Elektronik INT = (SELECT Id FROM dbo.Categories WHERE Ad = N'Elektronik');
    DECLARE @Ofis INT = (SELECT Id FROM dbo.Categories WHERE Ad = N'Ofis Malzemeleri');
    DECLARE @Endustriyel INT = (SELECT Id FROM dbo.Categories WHERE Ad = N'Endüstriyel Ekipman');
    DECLARE @Gida INT = (SELECT Id FROM dbo.Categories WHERE Ad = N'Gıda');
    DECLARE @Temizlik INT = (SELECT Id FROM dbo.Categories WHERE Ad = N'Temizlik Ürünleri');

    INSERT INTO dbo.Products (UrunKodu, UrunAdi, Aciklama, Marka, UreticiKodu, OzelKod1, OzelKod2, ResimUrl, StokMiktari, KritikStokSeviyesi, Fiyat, CategoryId) VALUES
    (N'ELK-001', N'Kablosuz Mouse', N'2.4GHz kablosuz optik mouse', N'Logitech', N'M185', N'SRF-01', NULL, N'/images/products/elk-001.png', 120, 15, 249.90, @Elektronik),
    (N'ELK-002', N'USB-C Hub 7in1', N'HDMI, USB3.0, kart okuyucu içeren çoklu hub', N'Anker', N'A83XX', NULL, NULL, NULL, 8, 10, 899.00, @Elektronik),
    (N'ELK-003', N'Mekanik Klavye', N'Blue switch, TR-Q', N'Redragon', N'K552', N'SRF-02', NULL, N'/images/products/elk-003.png', 0, 5, 749.50, @Elektronik),
    (N'OFS-001', N'A4 Fotokopi Kağıdı (5 Paket)', N'80gr, 500 yaprak/paket', N'Navigator', NULL, NULL, NULL, NULL, 300, 50, 189.90, @Ofis),
    (N'OFS-002', N'Toner Kartuşu HP 85A', N'Orijinal siyah toner', N'HP', N'CE285A', N'ST-85A', NULL, NULL, 4, 5, 1299.00, @Ofis),
    (N'END-001', N'Endüstriyel Eldiven (Çift)', N'Kesime dayanıklı, kevlar takviyeli', N'3M', N'KV-100', NULL, NULL, NULL, 60, 20, 99.90, @Endustriyel),
    (N'END-002', N'İş Güvenliği Bareti', N'Sarı, ayarlanabilir bant', N'MSA', N'V-Gard', NULL, NULL, NULL, 25, 10, 349.00, @Endustriyel),
    (N'GID-001', N'Filtre Kahve 1kg', N'Öğütülmüş, ofis paketi', N'Kurukahveci', NULL, N'ST-KAH1', NULL, NULL, 45, 15, 429.00, @Gida),
    (N'TMZ-001', N'Yüzey Dezenfektanı 5L', N'Alkol bazlı, çok yüzeyli kullanım', N'Domestos', NULL, NULL, NULL, NULL, 2, 10, 259.90, @Temizlik),
    (N'TMZ-002', N'Kağıt Havlu (12li Koli)', N'Çift katlı, 100 yaprak/rulo', N'Selpak', NULL, NULL, NULL, NULL, 90, 20, 549.00, @Temizlik);
END
GO

-- Dinamik grid kolon konfigürasyonu
IF NOT EXISTS (SELECT 1 FROM dbo.ProductGridColumns)
BEGIN
    INSERT INTO dbo.ProductGridColumns (ColumnKey, DisplayName, OrderIndex, RenderType, IsVisible, ShowOnDesktop, ShowOnTablet, ShowOnMobile, Width, Alignment) VALUES
    (N'ResimUrl',    N'Görsel',        1, N'Image',      1, 1, 1, 0, N'70px',  N'center'),
    (N'UrunKodu',    N'Ürün Kodu',     2, N'Text',       1, 1, 1, 1, N'110px', N'left'),
    (N'UrunAdi',     N'Ürün Adı',      3, N'Text',       1, 1, 1, 1, NULL,     N'left'),
    (N'Marka',       N'Marka',         4, N'Text',       1, 1, 1, 0, N'120px', N'left'),
    (N'StokDurumu',  N'Stok Durumu',   5, N'StockBadge', 1, 1, 1, 1, N'110px', N'center'),
    (N'Fiyat',       N'Fiyat',         6, N'Currency',   1, 1, 1, 1, N'110px', N'right'),
    (N'AdetGiris',   N'Adet',          7, N'QtyInput',   1, 1, 1, 1, N'160px', N'center');
END
GO

-- Ana sayfa slider'ı için örnek banner'lar
IF NOT EXISTS (SELECT 1 FROM dbo.Banners)
BEGIN
    INSERT INTO dbo.Banners (Baslik, ResimUrl, Link, Sira, IsActive) VALUES
    (N'Mini B2B Kataloğuna Hoş Geldiniz', NULL, NULL, 1, 1),
    (N'Toplu Alımlarda Avantajlı Fiyatlar', NULL, NULL, 2, 1),
    (N'Siparişlerinizi Anlık Takip Edin', NULL, NULL, 3, 1);
END
GO
