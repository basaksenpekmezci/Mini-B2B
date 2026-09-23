using System.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface ICartRepository
{
    Task<int> GetOrCreateCartIdAsync(int userId);
    Task<IEnumerable<CartItem>> GetItemsAsync(int cartId);

    /// <summary>
    /// Sipariş oluşturma transaction'ı içinde çağrılır: sepet kalemlerini, ilgili ürün satırlarını
    /// WITH (UPDLOCK, ROWLOCK) ile kilitleyerek TEK sorguda getirir. Böylece stok kontrolü, fiyat ve
    /// ürün adı snapshot'ı aynı, kilitli okumadan gelir — ayrıca ürün başına ek sorgu atılmaz
    /// (bkz. OrderService.CreateOrderFromCartAsync).
    /// </summary>
    Task<IEnumerable<CartItem>> GetItemsForOrderAsync(IDbConnection connection, IDbTransaction transaction, int cartId);

    Task UpsertItemAsync(int cartId, int productId, int adet);
    Task<bool> SetItemQuantityAsync(int cartId, int productId, int adet);
    Task RemoveItemAsync(int cartId, int productId);

    /// <summary>
    /// Sipariş oluşturma transaction'ı içinde sepeti temizlemek için kullanılır (bkz. OrderService).
    /// </summary>
    Task ClearAsync(IDbConnection connection, IDbTransaction transaction, int cartId);
}
