/*
    ALTER script: Sipariş numarası üretimini zaman damgasından SQL Server SEQUENCE'ına taşır.
    01_schema.sql'i baştan çalıştırmadan, MEVCUT bir veritabanına uygulamak için kullanılır.
    Idempotent'tir; birden fazla kez çalıştırılabilir.

    Çalıştırma: sqlcmd -S <server> -i db/03_add_siparisno_sequence.sql
*/

USE MiniB2B;
GO

IF OBJECT_ID('dbo.SiparisNoSequence', 'SO') IS NOT NULL DROP SEQUENCE dbo.SiparisNoSequence;
GO
CREATE SEQUENCE dbo.SiparisNoSequence
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO
