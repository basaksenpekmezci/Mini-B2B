using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Domain;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Areas.Admin.Controllers;

// Mağaza tarafındaki dinamik ürün grid'inin kolonlarını (hangi alan, sırası, render tipi, hizalama,
// genişlik, hangi cihazda görünür) SQL yazmadan yönetmek için.
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GridColumnsController : Controller
{
    private readonly IProductGridColumnService _service;

    public GridColumnsController(IProductGridColumnService service)
    {
        _service = service;
    }

    // GET /Admin/GridColumns
    public async Task<IActionResult> Index()
    {
        var columns = await _service.GetAllOrderedAsync();
        return View(columns);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View(new ProductGridColumn
        {
            IsVisible = true,
            ShowOnDesktop = true,
            ShowOnTablet = true,
            ShowOnMobile = true,
            Alignment = "left",
            RenderType = nameof(GridRenderType.Text)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductGridColumn column)
    {
        var result = await _service.CreateAsync(column);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Kolon oluşturulamadı.");
            PopulateDropdowns();
            return View(column);
        }

        TempData["Success"] = $"\"{column.DisplayName}\" kolonu oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var column = await _service.GetByIdAsync(id);
        if (column is null) return NotFound();

        PopulateDropdowns();
        return View(column);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductGridColumn column)
    {
        if (id != column.Id) return BadRequest();

        var result = await _service.UpdateAsync(column);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Kolon güncellenemedi.");
            PopulateDropdowns();
            return View(column);
        }

        TempData["Success"] = $"\"{column.DisplayName}\" kolonu güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropdowns()
    {
        ViewBag.RenderTypes = Enum.GetNames<GridRenderType>();
        ViewBag.Alignments = new[] { "left", "center", "right" };
    }
}
