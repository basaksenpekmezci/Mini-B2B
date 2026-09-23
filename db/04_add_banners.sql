/*
    ALTER script: MEVCUT bir veritabanına Banners tablosunu (ana sayfa slider'ı için) ekler.
    01_schema.sql'i baştan çalıştırmadan (veriyi silmeden) kullanılır. Idempotent'tir:
    tablo zaten varsa dokunmaz, örnek veri de sadece tablo boşsa eklenir.

    Çalıştırma: sqlcmd -S <server> -i db/04_add_banners.sql
*/

USE MiniB2B;
GO

IF OBJECT_ID('dbo.Banners', 'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Banners)
BEGIN
    INSERT INTO dbo.Banners (Baslik, ResimUrl, Link, Sira, IsActive) VALUES
    (N'Mini B2B Kataloğuna Hoş Geldiniz', NULL, NULL, 1, 1),
    (N'Toplu Alımlarda Avantajlı Fiyatlar', NULL, NULL, 2, 1),
    (N'Siparişlerinizi Anlık Takip Edin', NULL, NULL, 3, 1);
END
GO
