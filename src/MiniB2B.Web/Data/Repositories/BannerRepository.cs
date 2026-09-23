using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly DapperContext _context;

    public BannerRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Banner>> GetActiveOrderedAsync()
    {
        const string sql = @"
            SELECT Id, Baslik, ResimUrl, Link, Sira, IsActive
            FROM dbo.Banners
            WHERE IsActive = 1
            ORDER BY Sira;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Banner>(sql);
    }

    public async Task<IEnumerable<Banner>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Baslik, ResimUrl, Link, Sira, IsActive
            FROM dbo.Banners
            ORDER BY Sira;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Banner>(sql);
    }

    public async Task<Banner?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Baslik, ResimUrl, Link, Sira, IsActive
            FROM dbo.Banners
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Banner>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Banner banner)
    {
        const string sql = @"
            INSERT INTO dbo.Banners (Baslik, ResimUrl, Link, Sira, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Baslik, @ResimUrl, @Link, @Sira, @IsActive);";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, banner);
    }

    public async Task UpdateAsync(Banner banner)
    {
        const string sql = @"
            UPDATE dbo.Banners
            SET Baslik = @Baslik,
                ResimUrl = @ResimUrl,
                Link = @Link,
                Sira = @Sira,
                IsActive = @IsActive
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, banner);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Banners WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
