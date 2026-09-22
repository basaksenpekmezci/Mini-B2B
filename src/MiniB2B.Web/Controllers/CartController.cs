using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CartController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    // GET /Cart
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int adet, string? returnUrl)
    {
        var result = await _cartService.AddToCartAsync(GetUserId(), productId, adet);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Ürün sepete eklendi."
            : result.ErrorMessage;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int productId, int adet)
    {
        var result = await _cartService.UpdateQuantityAsync(GetUserId(), productId, adet);
        if (!result.Success) TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _cartService.RemoveAsync(GetUserId(), productId);
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Checkout — sepeti siparişe dönüştürür (backend stok kontrolü OrderService içinde yapılır).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var result = await _orderService.CreateOrderFromCartAsync(GetUserId());
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Siparişiniz oluşturuldu. Sipariş No: {result.Order!.SiparisNo}";
        return RedirectToAction("Index", "Orders");
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
