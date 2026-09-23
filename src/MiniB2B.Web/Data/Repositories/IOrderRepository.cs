using System.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IOrderRepository
{
    /// <summary>
    /// Sipariş oluşturma transaction'ı içinde (stok kontrolü ile aynı bağlantı/transaction üzerinde) çalışır.
    /// </summary>
    Task<int> CreateOrderAsync(IDbConnection connection, IDbTransaction transaction, Order order);
    Task AddOrderItemAsync(IDbConnection connection, IDbTransaction transaction, int orderId, OrderItem item);

    Task<IEnumerable<Order>> GetOrdersForUserAsync(int userId);
    Task<Order?> GetOrderDetailAsync(int orderId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();

    /// <summary>
    /// Sadece "Beklemede" durumundaki siparişi hedef duruma taşır (WHERE Durum = 'Beklemede' koşuluyla);
    /// zaten sonuçlanmış bir sipariş için 0 satır etkilenir ve false döner. Sipariş oluşturma ile aynı
    /// desende, dışarıdan verilen transaction üzerinde çalışır (bkz. OrderService.UpdateStatusAsync).
    /// </summary>
    Task<bool> TryUpdateStatusAsync(IDbConnection connection, IDbTransaction transaction, int orderId, string durum);

    /// <summary>
    /// Reddedilen bir siparişin kalemlerindeki adetleri, tek bir set-based UPDATE...JOIN sorgusuyla
    /// (ürün başına ayrı sorgu atmadan) ilgili ürünlerin stoğuna geri ekler.
    /// </summary>
    Task RestoreStockForOrderAsync(IDbConnection connection, IDbTransaction transaction, int orderId);
}
