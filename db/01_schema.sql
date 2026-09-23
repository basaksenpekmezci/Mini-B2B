/*
    Mini B2B E-Ticaret Projesi - Veritabanı Şeması
    Hedef: SQL Server (2019+ / LocalDB / Express)
    Çalıştırma: sqlcmd -S <server> -i 01_schema.sql
                veya SSMS'te açıp Execute (F5)
*/

IF DB_ID('MiniB2B') IS NULL
BEGIN
    CREATE DATABASE MiniB2B;
END
GO

USE MiniB2B;
GO

-- =========================================================
-- Users
-- =========================================================
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users
(
    Id              INT             IDENTITY(1,1)   NOT NULL,
    Ad              NVARCHAR(100)   NOT NULL,
    Soyad           NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(256)   NOT NULL,
    Telefon         NVARCHAR(20)    NULL,
    Username        NVARCHAR(100)   NOT NULL,
    PasswordHash    VARBINARY(64)   NOT NULL,
    PasswordSalt    VARBINARY(32)   NOT NULL,
    IsAdmin         BIT             NOT NULL CONSTRAINT DF_Users_IsAdmin DEFAULT (0),
    CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);
GO

-- =========================================================
-- Categories
-- =========================================================
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO
CREATE TABLE dbo.Categories
(
    Id      INT             IDENTITY(1,1)   NOT NULL,
    Ad      NVARCHAR(150)   NOT NULL,

    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Categories_Ad UNIQUE (Ad)
);
GO

-- =========================================================
-- Products
-- =========================================================
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
GO
CREATE TABLE dbo.Products
(
    Id                      INT             IDENTITY(1,1)   NOT NULL,
    UrunKodu                NVARCHAR(50)    NOT NULL,
    UrunAdi                 NVARCHAR(250)   NOT NULL,
    Aciklama                NVARCHAR(MAX)   NULL,
    Marka                   NVARCHAR(150)   NULL,
    UreticiKodu             NVARCHAR(100)   NULL,
    OzelKod1                NVARCHAR(100)   NULL,
    OzelKod2                NVARCHAR(100)   NULL,
    ResimUrl                NVARCHAR(500)   NULL,
    StokMiktari             INT             NOT NULL CONSTRAINT DF_Products_Stok DEFAULT (0),
    KritikStokSeviyesi      INT             NOT NULL CONSTRAINT DF_Products_KritikStok DEFAULT (5),
    Fiyat                   DECIMAL(18,2)   NOT NULL CONSTRAINT DF_Products_Fiyat DEFAULT (0),
    CategoryId              INT             NULL,
    IsActive                BIT             NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
    CreatedAt               DATETIME2       NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Products_UrunKodu UNIQUE (UrunKodu),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT CK_Products_StokMiktari CHECK (StokMiktari >= 0),
    CONSTRAINT CK_Products_Fiyat CHECK (Fiyat >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Products_UrunAdi ON dbo.Products(UrunAdi);
CREATE NONCLUSTERED INDEX IX_Products_Marka ON dbo.Products(Marka);
GO

-- =========================================================
-- ProductGridColumns  (dinamik grid konfigürasyonu)
-- =========================================================
IF OBJECT_ID('dbo.ProductGridColumns', 'U') IS NOT NULL DROP TABLE dbo.ProductGridColumns;
GO
CREATE TABLE dbo.ProductGridColumns
(
    Id              INT             IDENTITY(1,1)   NOT NULL,
    ColumnKey       NVARCHAR(50)    NOT NULL,   -- Product nesnesindeki property adı (reflection ile okunur)
    DisplayName     NVARCHAR(100)   NOT NULL,
    OrderIndex      INT             NOT NULL,
    RenderType      NVARCHAR(30)    NOT NULL,   -- Text | Image | Number | Currency | StockBadge | QtyInput
    IsVisible       BIT             NOT NULL CONSTRAINT DF_PGC_IsVisible DEFAULT (1),
    ShowOnDesktop   BIT             NOT NULL CONSTRAINT DF_PGC_Desktop DEFAULT (1),
    ShowOnTablet    BIT             NOT NULL CONSTRAINT DF_PGC_Tablet DEFAULT (1),
    ShowOnMobile    BIT             NOT NULL CONSTRAINT DF_PGC_Mobile DEFAULT (1),
    Width           NVARCHAR(20)    NULL,       -- ör: '80px', '20%'
    Alignment       NVARCHAR(10)    NOT NULL CONSTRAINT DF_PGC_Align DEFAULT ('left'), -- left|center|right

    CONSTRAINT PK_ProductGridColumns PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ProductGridColumns_ColumnKey UNIQUE (ColumnKey),
    CONSTRAINT CK_ProductGridColumns_RenderType CHECK (RenderType IN ('Text','Image','Number','Currency','StockBadge','QtyInput')),
    CONSTRAINT CK_ProductGridColumns_Alignment CHECK (Alignment IN ('left','center','right'))
);
GO

-- =========================================================
-- Banners  (ana sayfa slider'ı için admin panelinden yönetilen içerik)
-- =========================================================
IF OBJECT_ID('dbo.Banners', 'U') IS NOT NULL DROP TABLE dbo.Banners;
GO
CREATE TABLE dbo.Banners
(
    Id          INT             IDENTITY(1,1)   NOT NULL,
    Baslik      NVARCHAR(200)   NOT NULL,
    ResimUrl    NVARCHAR(500)   NULL,
    Link        NVARCHAR(500)   NULL,
    Sira        INT             NOT NULL CONSTRAINT DF_Banners_Sira DEFAULT (0),
    IsActive    BIT             NOT NULL CONSTRAINT DF_Banners_IsActive DEFAULT (1),

    CONSTRAINT PK_Banners PRIMARY KEY CLUSTERED (Id)
);
GO

-- =========================================================
-- Cart / CartItems
-- =========================================================
IF OBJECT_ID('dbo.CartItems', 'U') IS NOT NULL DROP TABLE dbo.CartItems;
IF OBJECT_ID('dbo.Cart', 'U') IS NOT NULL DROP TABLE dbo.Cart;
GO
CREATE TABLE dbo.Cart
(
    Id          INT             IDENTITY(1,1)   NOT NULL,
    UserId      INT             NOT NULL,
    CreatedAt   DATETIME2       NOT NULL CONSTRAINT DF_Cart_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Cart PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Cart_UserId UNIQUE (UserId),   -- her kullanıcının tek aktif sepeti var
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.CartItems
(
    Id          INT             IDENTITY(1,1)   NOT NULL,
    CartId      INT             NOT NULL,
    ProductId   INT             NOT NULL,
    Adet        INT             NOT NULL,

    CONSTRAINT PK_CartItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CartItems_Cart_Product UNIQUE (CartId, ProductId),  -- aynı ürün tekrar eklenirse adet güncellenir
    CONSTRAINT FK_CartItems_Cart FOREIGN KEY (CartId) REFERENCES dbo.Cart(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_CartItems_Adet CHECK (Adet > 0)
);
GO

-- =========================================================
-- Orders / OrderItems
-- =========================================================
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
GO

-- SiparisNo üretimi için: zaman damgası yerine, eşzamanlı sipariş oluşturmalarda bile çakışmayan,
-- SQL Server tarafından atomik şekilde artırılan bir SEQUENCE kullanılıyor (bkz. OrderService).
IF OBJECT_ID('dbo.SiparisNoSequence', 'SO') IS NOT NULL DROP SEQUENCE dbo.SiparisNoSequence;
GO
CREATE SEQUENCE dbo.SiparisNoSequence
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE dbo.Orders
(
    Id              INT             IDENTITY(1,1)   NOT NULL,
    SiparisNo       NVARCHAR(30)    NOT NULL,
    UserId          INT             NOT NULL,
    SiparisTarihi   DATETIME2       NOT NULL CONSTRAINT DF_Orders_Tarih DEFAULT (SYSUTCDATETIME()),
    Durum           NVARCHAR(20)    NOT NULL CONSTRAINT DF_Orders_Durum DEFAULT ('Beklemede'), -- Beklemede|Onaylandi|Reddedildi
    ToplamTutar     DECIMAL(18,2)   NOT NULL,

    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Orders_SiparisNo UNIQUE (SiparisNo),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Orders_Durum CHECK (Durum IN ('Beklemede','Onaylandi','Reddedildi'))
);
GO

CREATE NONCLUSTERED INDEX IX_Orders_UserId ON dbo.Orders(UserId);
GO

CREATE TABLE dbo.OrderItems
(
    Id              INT             IDENTITY(1,1)   NOT NULL,
    OrderId         INT             NOT NULL,
    ProductId       INT             NOT NULL,
    UrunKodu        NVARCHAR(50)    NOT NULL,   -- sipariş anındaki değer (snapshot)
    UrunAdi         NVARCHAR(250)   NOT NULL,   -- snapshot
    Adet            INT             NOT NULL,
    BirimFiyat      DECIMAL(18,2)   NOT NULL,   -- snapshot
    ToplamFiyat     DECIMAL(18,2)   NOT NULL,

    CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_OrderItems_Adet CHECK (Adet > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
GO
