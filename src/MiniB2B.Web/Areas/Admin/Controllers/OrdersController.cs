using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /Admin/Orders
    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return View(orders);
    }

    // GET /Admin/Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderDetailAsync(id);
        if (order is null) return NotFound();
        return View(order);
    }

    // POST /Admin/Orders/UpdateStatus — durum değişikliği kullanıcının "Siparişlerim" ekranına da yansır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string durum)
    {
        try
        {
            var result = await _orderService.UpdateStatusAsync(id, durum);
            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Sipariş durumu güncellendi."
                : result.ErrorMessage;
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
