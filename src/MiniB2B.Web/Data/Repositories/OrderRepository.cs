using System.Data;
using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DapperContext _context;

    public OrderRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<int> CreateOrderAsync(IDbConnection connection, IDbTransaction transaction, Order order)
    {
        const string sql = @"
            INSERT INTO dbo.Orders (SiparisNo, UserId, SiparisTarihi, Durum, ToplamTutar)
            OUTPUT INSERTED.Id
            VALUES (@SiparisNo, @UserId, SYSUTCDATETIME(), @Durum, @ToplamTutar);";

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, order, transaction));
    }

    public async Task AddOrderItemAsync(IDbConnection connection, IDbTransaction transaction, int orderId, OrderItem item)
    {
        const string sql = @"
            INSERT INTO dbo.OrderItems (OrderId, ProductId, UrunKodu, UrunAdi, Adet, BirimFiyat, ToplamFiyat)
            VALUES (@OrderId, @ProductId, @UrunKodu, @UrunAdi, @Adet, @BirimFiyat, @ToplamFiyat);";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            OrderId = orderId,
            item.ProductId,
            item.UrunKodu,
            item.UrunAdi,
            item.Adet,
            item.BirimFiyat,
            item.ToplamFiyat
        }, transaction));
    }

    public async Task<IEnumerable<Order>> GetOrdersForUserAsync(int userId)
    {
        const string sql = @"
            SELECT Id, SiparisNo, UserId, SiparisTarihi, Durum, ToplamTutar
            FROM dbo.Orders
            WHERE UserId = @UserId
            ORDER BY SiparisTarihi DESC;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Order>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Sipariş başlığını (kullanıcı adıyla birlikte) ve kalemlerini tek round-trip'te getirir.
    /// </summary>
    public async Task<Order?> GetOrderDetailAsync(int orderId)
    {
        const string sql = @"
            SELECT o.Id, o.SiparisNo, o.UserId, (u.Ad + N' ' + u.Soyad) AS KullaniciAdSoyad,
                   o.SiparisTarihi, o.Durum, o.ToplamTutar
            FROM dbo.Orders o
            JOIN dbo.Users u ON u.Id = o.UserId
            WHERE o.Id = @OrderId;

            SELECT Id, OrderId, ProductId, UrunKodu, UrunAdi, Adet, BirimFiyat, ToplamFiyat
            FROM dbo.OrderItems
            WHERE OrderId = @OrderId
            ORDER BY Id;";

        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, new { OrderId = orderId });

        var order = await multi.ReadSingleOrDefaultAsync<Order>();
        if (order is null) return null;

        order.Kalemler = (await multi.ReadAsync<OrderItem>()).ToList();
        return order;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        const string sql = @"
            SELECT o.Id, o.SiparisNo, o.UserId, (u.Ad + N' ' + u.Soyad) AS KullaniciAdSoyad,
                   o.SiparisTarihi, o.Durum, o.ToplamTutar
            FROM dbo.Orders o
            JOIN dbo.Users u ON u.Id = o.UserId
            ORDER BY o.SiparisTarihi DESC;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Order>(sql);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string durum)
    {
        const string sql = "UPDATE dbo.Orders SET Durum = @Durum WHERE Id = @OrderId;";

        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { OrderId = orderId, Durum = durum });
        return affected > 0;
    }
}
