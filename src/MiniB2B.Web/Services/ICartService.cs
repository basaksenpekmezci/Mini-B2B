using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class CartActionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface ICartService
{
    Task<Cart> GetCartAsync(int userId);
    Task<CartActionResult> AddToCartAsync(int userId, int productId, int adet);
    Task<CartActionResult> UpdateQuantityAsync(int userId, int productId, int adet);
    Task RemoveAsync(int userId, int productId);
}
