/*
    ALTER script: MEVCUT bir veritabanına sipariş numarası SEQUENCE'ını ekler.
    01_schema.sql'i baştan çalıştırmadan (veriyi silmeden) kullanılır.

    Gerçek idempotent: SEQUENCE zaten varsa HİÇBİR ŞEY YAPMAZ (DROP edip sıfırlamaz).
    Bu script daha önce çalıştırılmış ve o sırada zaten yeni formatta siparişler
    oluşturulmuşsa, tekrar çalıştırıldığında sequence'ı 1'e resetlemek UNIQUE (SiparisNo)
    ihlaline yol açardı (SP000001 zaten kullanılmış olabilir) — bu yüzden IF NOT EXISTS
    ile korunuyor.

    Sequence ilk kez oluşturulurken, Orders tablosunda zaten 'SP' + 6 haneli formatta
    (örn. SP000042) kayıtlar varsa, sequence en büyük mevcut numaradan bir sonrasından
    başlar; böylece eski (zaman damgalı) sipariş no'larıyla da, yeni formatta zaten
    oluşmuş kayıtlarla da çakışma olmaz.

    Çalıştırma: sqlcmd -S <server> -i db/03_add_siparisno_sequence.sql
*/

USE MiniB2B;
GO

IF OBJECT_ID('dbo.SiparisNoSequence', 'SO') IS NULL
BEGIN
    DECLARE @NextStart INT;

    SELECT @NextStart = ISNULL(MAX(TRY_CAST(SUBSTRING(SiparisNo, 3, 6) AS INT)), 0) + 1
    FROM dbo.Orders
    WHERE SiparisNo LIKE 'SP[0-9][0-9][0-9][0-9][0-9][0-9]';

    DECLARE @Sql NVARCHAR(300) = N'CREATE SEQUENCE dbo.SiparisNoSequence AS INT START WITH '
        + CAST(@NextStart AS NVARCHAR(20)) + N' INCREMENT BY 1 NO CYCLE;';
    EXEC sp_executesql @Sql;

    PRINT N'SiparisNoSequence oluşturuldu, başlangıç değeri: ' + CAST(@NextStart AS NVARCHAR(20));
END
ELSE
BEGIN
    PRINT N'SiparisNoSequence zaten mevcut, dokunulmadı.';
END
GO
