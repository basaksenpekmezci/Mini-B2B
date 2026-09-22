using System.Data;
using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DapperContext _context;

    public ProductRepository(DapperContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ürün adı, kodu, marka, üretici kodu, özel kodlar ve açıklama üzerinde arama yapar.
    /// Ödev gereksinimi: sayısal alanlar (stok, fiyat) hariç tüm metinsel/kod kolonlarında arama.
    /// SQL tarafında tek sorgu ile yapılır; tüm kayıtları çekip uygulamada filtrelemek YAPILMAZ.
    /// </summary>
    public async Task<IEnumerable<Product>> SearchAsync(string? searchTerm)
    {
        const string sql = @"
            SELECT p.Id, p.UrunKodu, p.UrunAdi, p.Aciklama, p.Marka, p.UreticiKodu,
                   p.OzelKod1, p.OzelKod2, p.ResimUrl, p.StokMiktari, p.KritikStokSeviyesi,
                   p.Fiyat, p.CategoryId, c.Ad AS CategoryAdi, p.IsActive, p.CreatedAt
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId
            WHERE p.IsActive = 1
              AND (
                    @Term IS NULL
                 OR p.UrunAdi     LIKE '%' + @Term + '%'
                 OR p.UrunKodu    LIKE '%' + @Term + '%'
                 OR p.Marka       LIKE '%' + @Term + '%'
                 OR p.UreticiKodu LIKE '%' + @Term + '%'
                 OR p.OzelKod1    LIKE '%' + @Term + '%'
                 OR p.OzelKod2    LIKE '%' + @Term + '%'
                 OR p.Aciklama    LIKE '%' + @Term + '%'
              )
            ORDER BY p.UrunAdi;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Product>(sql, new { Term = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim() });
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT p.Id, p.UrunKodu, p.UrunAdi, p.Aciklama, p.Marka, p.UreticiKodu,
                   p.OzelKod1, p.OzelKod2, p.ResimUrl, p.StokMiktari, p.KritikStokSeviyesi,
                   p.Fiyat, p.CategoryId, c.Ad AS CategoryAdi, p.IsActive, p.CreatedAt
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId
            WHERE p.Id = @Id;";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<bool> UrunKoduExistsAsync(string urunKodu, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1) FROM dbo.Products
            WHERE UrunKodu = @UrunKodu AND (@ExcludeId IS NULL OR Id <> @ExcludeId);";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UrunKodu = urunKodu, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<int> CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO dbo.Products
                (UrunKodu, UrunAdi, Aciklama, Marka, UreticiKodu, OzelKod1, OzelKod2,
                 ResimUrl, StokMiktari, KritikStokSeviyesi, Fiyat, CategoryId, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES
                (@UrunKodu, @UrunAdi, @Aciklama, @Marka, @UreticiKodu, @OzelKod1, @OzelKod2,
                 @ResimUrl, @StokMiktari, @KritikStokSeviyesi, @Fiyat, @CategoryId, 1, SYSUTCDATETIME());";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, product);
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE dbo.Products
            SET UrunKodu = @UrunKodu,
                UrunAdi = @UrunAdi,
                Aciklama = @Aciklama,
                Marka = @Marka,
                UreticiKodu = @UreticiKodu,
                OzelKod1 = @OzelKod1,
                OzelKod2 = @OzelKod2,
                ResimUrl = @ResimUrl,
                StokMiktari = @StokMiktari,
                KritikStokSeviyesi = @KritikStokSeviyesi,
                Fiyat = @Fiyat,
                CategoryId = @CategoryId
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, product);
    }

    public async Task<int> GetStokMiktariForUpdateAsync(IDbConnection connection, IDbTransaction transaction, int productId)
    {
        // UPDLOCK/ROWLOCK: aynı ürün için eşzamanlı siparişlerde "race condition" ile
        // stok kontrolünün atlatılmasını (iki siparişin aynı anda "yeterli stok var" görmesini) engeller.
        const string sql = @"
            SELECT StokMiktari
            FROM dbo.Products WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @ProductId;";

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { ProductId = productId }, transaction));
    }

    public async Task DecrementStokAsync(IDbConnection connection, IDbTransaction transaction, int productId, int adet)
    {
        const string sql = @"
            UPDATE dbo.Products
            SET StokMiktari = StokMiktari - @Adet
            WHERE Id = @ProductId;";

        await connection.ExecuteAsync(new CommandDefinition(sql, new { ProductId = productId, Adet = adet }, transaction));
    }
}
