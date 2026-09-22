using Dapper;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Data;

/// <summary>
/// Uygulama ayağa kalktığında çalışır. Şifre hash'i .NET tarafında üretilmesi
/// gerektiği için (SQL script içinde PBKDF2 üretilemez), varsayılan admin
/// kullanıcısı burada, kod üzerinden, idempotent şekilde oluşturulur.
/// Tablo şemasının kendisi 01_schema.sql / 02_seed.sql ile elle kurulmalıdır
/// (bkz. README) — bu sınıf DDL çalıştırmaz, sadece admin kullanıcısını seed eder.
/// </summary>
public static class DbInitializer
{
    private const string AdminUsername = "admin";
    private const string AdminEmail = "admin@minib2b.local";
    private const string AdminDefaultPassword = "Admin123!";

    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DapperContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        using var connection = context.CreateConnection();

        const string checkSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username;";
        var exists = await connection.ExecuteScalarAsync<int>(checkSql, new { Username = AdminUsername });
        if (exists > 0) return;

        var (hash, salt) = passwordHasher.HashPassword(AdminDefaultPassword);

        const string insertSql = @"
            INSERT INTO dbo.Users (Ad, Soyad, Email, Telefon, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAt)
            VALUES (N'Sistem', N'Yöneticisi', @Email, NULL, @Username, @Hash, @Salt, 1, SYSUTCDATETIME());";

        await connection.ExecuteAsync(insertSql, new
        {
            Email = AdminEmail,
            Username = AdminUsername,
            Hash = hash,
            Salt = salt
        });
    }
}
