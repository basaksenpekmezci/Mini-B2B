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
    /// Sepet kalemlerini, ilgili ürün satırlarını UPDLOCK/ROWLOCK ile kilitleyerek TEK sorguda ve
    /// transaction içinde okur (fiyat/stok snapshot'ı bu kilitli okumadan gelir, transaction öncesi
    /// alınmış bir veriden değil). Pasif ürün varsa ya da herhangi bir üründe stok yetersizse hiçbir
    /// değişiklik kalıcı olmaz (rollback); yeterliyse siparişi ve kalemlerini oluşturur, stoğu düşer
    /// ve sepeti temizler.
    /// </summary>
    public async Task<OrderCreateResult> CreateOrderFromCartAsync(int userId)
    {
        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);

        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var items = (await _cartRepository.GetItemsForOrderAsync(connection, transaction, cartId)).ToList();

            if (items.Count == 0)
            {
                transaction.Rollback();
                return new OrderCreateResult { Success = false, ErrorMessage = "Sepetiniz boş." };
            }

            var inactiveItem = items.FirstOrDefault(i => !i.IsActive);
            if (inactiveItem is not null)
            {
                transaction.Rollback();
                return new OrderCreateResult
                {
                    Success = false,
                    ErrorMessage = $"\"{inactiveItem.UrunAdi}\" artık satışta değil. Lütfen sepetinizden çıkarıp tekrar deneyin."
                };
            }

            foreach (var item in items)
            {
                if (item.MevcutStok < item.Adet)
                {
                    transaction.Rollback();
                    return new OrderCreateResult
                    {
                        Success = false,
                        ErrorMessage = $"\"{item.UrunAdi}\" için yeterli stok bulunmamaktadır. Mevcut stok: {item.MevcutStok}."
                    };
                }
            }

            var nextNo = await _orderRepository.GetNextSiparisNoAsync(connection, transaction);
            var order = new Order
            {
                SiparisNo = $"SP{nextNo:D6}",
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

    /// <summary>
    /// Sadece "Beklemede" durumundaki bir siparişin durumu değiştirilebilir (zaten sonuçlanmış bir
    /// siparişte anlaşılır bir hata döner). "Reddedildi" durumuna geçişte, sipariş kalemlerindeki
    /// adetler aynı transaction içinde tek bir set-based UPDATE ile ürünlerin stoğuna geri eklenir.
    /// </summary>
    public async Task<OrderStatusUpdateResult> UpdateStatusAsync(int orderId, string durum)
    {
        if (!AllowedStatusChanges.Contains(durum))
        {
            throw new ArgumentException($"Geçersiz sipariş durumu: {durum}", nameof(durum));
        }

        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var updated = await _orderRepository.TryUpdateStatusAsync(connection, transaction, orderId, durum);
            if (!updated)
            {
                transaction.Rollback();
                return new OrderStatusUpdateResult
                {
                    Success = false,
                    ErrorMessage = "Bu sipariş zaten sonuçlanmış (Beklemede durumunda değil); durumu tekrar değiştirilemez."
                };
            }

            if (durum == "Reddedildi")
            {
                await _orderRepository.RestoreStockForOrderAsync(connection, transaction, orderId);
            }

            transaction.Commit();
            return new OrderStatusUpdateResult { Success = true };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
