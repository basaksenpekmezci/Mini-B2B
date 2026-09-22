using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<Cart> GetCartAsync(int userId)
    {
        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);
        var items = await _cartRepository.GetItemsAsync(cartId);

        return new Cart
        {
            Id = cartId,
            UserId = userId,
            Items = items.ToList()
        };
    }

    public async Task<CartActionResult> AddToCartAsync(int userId, int productId, int adet)
    {
        if (adet <= 0)
        {
            return new CartActionResult { Success = false, ErrorMessage = "Adet 1 veya daha büyük olmalıdır." };
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null || !product.IsActive)
        {
            return new CartActionResult { Success = false, ErrorMessage = "Ürün bulunamadı." };
        }

        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);
        await _cartRepository.UpsertItemAsync(cartId, productId, adet);
        return new CartActionResult { Success = true };
    }

    public async Task<CartActionResult> UpdateQuantityAsync(int userId, int productId, int adet)
    {
        if (adet <= 0)
        {
            return new CartActionResult { Success = false, ErrorMessage = "Adet 1 veya daha büyük olmalıdır." };
        }

        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);
        var updated = await _cartRepository.SetItemQuantityAsync(cartId, productId, adet);
        if (!updated)
        {
            return new CartActionResult { Success = false, ErrorMessage = "Sepette bu ürün bulunamadı." };
        }

        return new CartActionResult { Success = true };
    }

    public async Task RemoveAsync(int userId, int productId)
    {
        var cartId = await _cartRepository.GetOrCreateCartIdAsync(userId);
        await _cartRepository.RemoveItemAsync(cartId, productId);
    }
}
