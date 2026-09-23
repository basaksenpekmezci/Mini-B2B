using System.Data;
using Dapper;
using MiniB2B.Web.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DapperContext _context;

    public ProductRepository(DapperContext context)
    {
        _context = context;
    }

    // Arama + kategori/marka filtresi + sayfa kayıtları ve toplam sayı sorgularında ortak WHERE
    // koşulu: sayısal alanlar (stok, fiyat) HARİÇ tüm metinsel/kod kolonlarında arama yapar
    // (ödev gereksinimi), kategori/marka/aktiflik filtreleri ile birlikte çalışır.
    private const string FilterWhereClause = @"
        WHERE (@IsActiveFilter IS NULL OR p.IsActive = @IsActiveFilter)
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@Marka IS NULL OR p.Marka = @Marka)
          AND (
                @Term IS NULL
             OR p.UrunAdi     LIKE '%' + @Term + '%'
             OR p.UrunKodu    LIKE '%' + @Term + '%'
             OR p.Marka       LIKE '%' + @Term + '%'
             OR p.UreticiKodu LIKE '%' + @Term + '%'
             OR p.OzelKod1    LIKE '%' + @Term + '%'
             OR p.OzelKod2    LIKE '%' + @Term + '%'
             OR p.Aciklama    LIKE '%' + @Term + '%'
          )";

    /// <summary>
    /// Arama + kategori/marka filtresi + sunucu taraflı sayfalama. Sayfa kayıtları ve toplam kayıt
    /// sayısı, tüm kayıtlar uygulamaya çekilmeden, tek round-trip'te (QueryMultiple) SQL tarafından
    /// hesaplanır (OFFSET/FETCH + COUNT).
    /// </summary>
    public async Task<PagedResult<Product>> SearchPagedAsync(string? searchTerm, int? categoryId, string? marka, bool? isActiveFilter, int page, int pageSize)
    {
        var sql = $@"
            SELECT p.Id, p.UrunKodu, p.UrunAdi, p.Aciklama, p.Marka, p.UreticiKodu,
                   p.OzelKod1, p.OzelKod2, p.ResimUrl, p.StokMiktari, p.KritikStokSeviyesi,
                   p.Fiyat, p.CategoryId, c.Ad AS CategoryAdi, p.IsActive, p.CreatedAt
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId
            {FilterWhereClause}
            ORDER BY p.UrunAdi
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1)
            FROM dbo.Products p
            {FilterWhereClause};";

        var safePage = Math.Max(1, page);
        var parameters = new
        {
            Term = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
            CategoryId = categoryId,
            Marka = string.IsNullOrWhiteSpace(marka) ? null : marka,
            IsActiveFilter = isActiveFilter,
            Offset = (safePage - 1) * pageSize,
            PageSize = pageSize
        };

        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, parameters);

        var items = (await multi.ReadAsync<Product>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = safePage,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<string>> GetDistinctBrandsAsync()
    {
        const string sql = @"
            SELECT DISTINCT Marka
            FROM dbo.Products
            WHERE Marka IS NOT NULL AND Marka <> ''
            ORDER BY Marka;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<string>(sql);
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

    public async Task DecrementStokAsync(IDbConnection connection, IDbTransaction transaction, int productId, int adet)
    {
        const string sql = @"
            UPDATE dbo.Products
            SET StokMiktari = StokMiktari - @Adet
            WHERE Id = @ProductId;";

        await connection.ExecuteAsync(new CommandDefinition(sql, new { ProductId = productId, Adet = adet }, transaction));
    }
}
