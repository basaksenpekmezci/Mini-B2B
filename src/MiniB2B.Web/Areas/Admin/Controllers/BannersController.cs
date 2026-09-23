using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Domain;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Areas.Admin.Controllers;

// Ana sayfadaki slider/banner alanının içeriğini (başlık, görsel, link, sıra, aktiflik) yönetir.
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BannersController : Controller
{
    private readonly IBannerService _service;

    public BannersController(IBannerService service)
    {
        _service = service;
    }

    // GET /Admin/Banners
    public async Task<IActionResult> Index()
    {
        var banners = await _service.GetAllOrderedAsync();
        return View(banners);
    }

    public IActionResult Create() => View(new Banner { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Banner banner)
    {
        var result = await _service.CreateAsync(banner);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Banner oluşturulamadı.");
            return View(banner);
        }

        TempData["Success"] = $"\"{banner.Baslik}\" banner'ı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var banner = await _service.GetByIdAsync(id);
        if (banner is null) return NotFound();
        return View(banner);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Banner banner)
    {
        if (id != banner.Id) return BadRequest();

        var result = await _service.UpdateAsync(banner);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Banner güncellenemedi.");
            return View(banner);
        }

        TempData["Success"] = $"\"{banner.Baslik}\" banner'ı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Banner silindi.";
        return RedirectToAction(nameof(Index));
    }
}
