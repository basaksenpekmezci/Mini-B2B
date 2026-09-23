using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class OrderCreateResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Order? Order { get; set; }
}

public class OrderStatusUpdateResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IOrderService
{
    Task<OrderCreateResult> CreateOrderFromCartAsync(int userId);

    Task<IEnumerable<Order>> GetOrdersForUserAsync(int userId);
    Task<Order?> GetOrderDetailForUserAsync(int orderId, int userId);

    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderDetailAsync(int orderId);
    Task<OrderStatusUpdateResult> UpdateStatusAsync(int orderId, string durum);
}
