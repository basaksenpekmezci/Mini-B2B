using System.Data;
using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly DapperContext _context;

    public CartRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<int> GetOrCreateCartIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();

        const string selectSql = "SELECT Id FROM dbo.Cart WHERE UserId = @UserId;";
        var existingId = await connection.ExecuteScalarAsync<int?>(selectSql, new { UserId = userId });
        if (existingId.HasValue) return existingId.Value;

        const string insertSql = @"
            INSERT INTO dbo.Cart (UserId, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, SYSUTCDATETIME());";

        return await connection.ExecuteScalarAsync<int>(insertSql, new { UserId = userId });
    }

    /// <summary>
    /// Sepet kalemlerini, listelemede gösterilecek ürün bilgileriyle (ad, kod, görsel, güncel fiyat/stok)
    /// tek sorguda join'leyerek getirir.
    /// </summary>
    public async Task<IEnumerable<CartItem>> GetItemsAsync(int cartId)
    {
        const string sql = @"
            SELECT ci.Id, ci.CartId, ci.ProductId, ci.Adet,
                   p.UrunKodu, p.UrunAdi, p.ResimUrl,
                   p.Fiyat AS BirimFiyat, p.StokMiktari AS MevcutStok
            FROM dbo.CartItems ci
            JOIN dbo.Products p ON p.Id = ci.ProductId
            WHERE ci.CartId = @CartId
            ORDER BY ci.Id;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CartItem>(sql, new { CartId = cartId });
    }

    public async Task<IEnumerable<CartItem>> GetItemsForOrderAsync(IDbConnection connection, IDbTransaction transaction, int cartId)
    {
        const string sql = @"
            SELECT ci.Id, ci.CartId, ci.ProductId, ci.Adet,
                   p.UrunKodu, p.UrunAdi, p.ResimUrl,
                   p.Fiyat AS BirimFiyat, p.StokMiktari AS MevcutStok, p.IsActive
            FROM dbo.CartItems ci
            JOIN dbo.Products p WITH (UPDLOCK, ROWLOCK) ON p.Id = ci.ProductId
            WHERE ci.CartId = @CartId
            ORDER BY ci.Id;";

        return await connection.QueryAsync<CartItem>(new CommandDefinition(sql, new { CartId = cartId }, transaction));
    }

    public async Task UpsertItemAsync(int cartId, int productId, int adet)
    {
        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @ProductId)
                UPDATE dbo.CartItems SET Adet = Adet + @Adet WHERE CartId = @CartId AND ProductId = @ProductId;
            ELSE
                INSERT INTO dbo.CartItems (CartId, ProductId, Adet) VALUES (@CartId, @ProductId, @Adet);";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId, Adet = adet });
    }

    public async Task<bool> SetItemQuantityAsync(int cartId, int productId, int adet)
    {
        const string sql = @"
            UPDATE dbo.CartItems SET Adet = @Adet
            WHERE CartId = @CartId AND ProductId = @ProductId;";

        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId, Adet = adet });
        return affected > 0;
    }

    public async Task RemoveItemAsync(int cartId, int productId)
    {
        const string sql = "DELETE FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @ProductId;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId });
    }

    public async Task ClearAsync(IDbConnection connection, IDbTransaction transaction, int cartId)
    {
        const string sql = "DELETE FROM dbo.CartItems WHERE CartId = @CartId;";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { CartId = cartId }, transaction));
    }
}
