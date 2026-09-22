using MiniB2B.Web.Data;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class OrderService : IOrderService
{
    private static readonly string[] AllowedStatusChanges = { "Onaylandi", "Reddedildi" };

    private readonly DapperContext _context;
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        DapperContext context,
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _context = context;
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Sepetteki tüm ürünlerin stoğunu tek transaction içinde (UPDLOCK/ROWLOCK ile) kontrol eder,
    /// yeterliyse siparişi ve kalemlerini (ürün/fiyat snapshot'ı ile) oluşturur, stoğu düşer ve
    /// sepeti temizler. Herhangi bir üründe stok yetersizse hiçbir değişiklik kalıcı olmaz (rollback).
    /// </summary>
    public async Task<OrderCreateResult> CreateOrderFromCartAsync(int userId)
    {
        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);
        var items = (await _cartRepository.GetItemsAsync(cartId)).ToList();

        if (items.Count == 0)
        {
            return new OrderCreateResult { Success = false, ErrorMessage = "Sepetiniz boş." };
        }

        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var item in items)
            {
                var currentStok = await _productRepository.GetStokMiktariForUpdateAsync(connection, transaction, item.ProductId);
                if (currentStok < item.Adet)
                {
                    transaction.Rollback();
                    return new OrderCreateResult
                    {
                        Success = false,
                        ErrorMessage = $"\"{item.UrunAdi}\" için yeterli stok bulunmamaktadır. Mevcut stok: {currentStok}."
                    };
                }
            }

            var order = new Order
            {
                SiparisNo = GenerateSiparisNo(),
                UserId = userId,
                Durum = "Beklemede",
                ToplamTutar = items.Sum(i => i.ToplamFiyat)
            };
            var orderId = await _orderRepository.CreateOrderAsync(connection, transaction, order);

            foreach (var item in items)
            {
                await _orderRepository.AddOrderItemAsync(connection, transaction, orderId, new OrderItem
                {
                    ProductId = item.ProductId,
                    UrunKodu = item.UrunKodu,
                    UrunAdi = item.UrunAdi,
                    Adet = item.Adet,
                    BirimFiyat = item.BirimFiyat,
                    ToplamFiyat = item.ToplamFiyat
                });

                await _productRepository.DecrementStokAsync(connection, transaction, item.ProductId, item.Adet);
            }

            await _cartRepository.ClearAsync(connection, transaction, cartId);

            transaction.Commit();

            order.Id = orderId;
            return new OrderCreateResult { Success = true, Order = order };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<IEnumerable<Order>> GetOrdersForUserAsync(int userId) => _orderRepository.GetOrdersForUserAsync(userId);

    public async Task<Order?> GetOrderDetailForUserAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetOrderDetailAsync(orderId);
        return order is null || order.UserId != userId ? null : order;
    }

    public Task<IEnumerable<Order>> GetAllOrdersAsync() => _orderRepository.GetAllOrdersAsync();

    public Task<Order?> GetOrderDetailAsync(int orderId) => _orderRepository.GetOrderDetailAsync(orderId);

    public Task<bool> UpdateStatusAsync(int orderId, string durum)
    {
        if (!AllowedStatusChanges.Contains(durum))
        {
            throw new ArgumentException($"Geçersiz sipariş durumu: {durum}", nameof(durum));
        }

        return _orderRepository.UpdateStatusAsync(orderId, durum);
    }

    private static string GenerateSiparisNo() => $"SP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
}
