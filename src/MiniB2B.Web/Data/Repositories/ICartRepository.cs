using System.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface ICartRepository
{
    Task<int> GetOrCreateCartIdAsync(int userId);
    Task<IEnumerable<CartItem>> GetItemsAsync(int cartId);
    Task UpsertItemAsync(int cartId, int productId, int adet);
    Task<bool> SetItemQuantityAsync(int cartId, int productId, int adet);
    Task RemoveItemAsync(int cartId, int productId);

    /// <summary>
    /// Sipariş oluşturma transaction'ı içinde sepeti temizlemek için kullanılır (bkz. OrderService).
    /// </summary>
    Task ClearAsync(IDbConnection connection, IDbTransaction transaction, int cartId);
}
