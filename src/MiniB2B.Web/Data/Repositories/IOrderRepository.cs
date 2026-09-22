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
    Task<bool> UpdateStatusAsync(int orderId, string durum);
}
