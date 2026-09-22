using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Controllers;

// "Siparişlerim" — giriş yapmış kullanıcının kendi siparişlerini görüntülediği ekran.
[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /Orders
    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetOrdersForUserAsync(GetUserId());
        return View(orders);
    }

    // GET /Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderDetailForUserAsync(id, GetUserId());
        if (order is null) return NotFound();
        return View(order);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
