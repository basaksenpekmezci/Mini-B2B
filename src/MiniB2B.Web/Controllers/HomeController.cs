using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Models;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Controllers;

public class HomeController : Controller
{
    private const int PageSize = 20;

    private readonly IProductService _productService;
    private readonly IProductGridColumnRepository _gridColumnRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBannerService _bannerService;

    public HomeController(
        IProductService productService,
        IProductGridColumnRepository gridColumnRepository,
        ICategoryRepository categoryRepository,
        IBannerService bannerService)
    {
        _productService = productService;
        _gridColumnRepository = gridColumnRepository;
        _categoryRepository = categoryRepository;
        _bannerService = bannerService;
    }

    // GET /  veya  /Home/Index?search=...&categoryId=...&marka=...&page=...
    // Dinamik ürün grid'i: kolonlar (hangi alan, sıra, render tipi, cihaz görünürlüğü) veritabanından
    // (ProductGridColumns) okunur; her hücrenin değeri ProductGridRenderer ile reflection üzerinden
    // çözülür. Ürün listesi arama + kategori/marka filtresi + sayfalama ile SQL tarafından gelir
    // (mağaza her zaman sadece aktif ürünleri gösterir).
    public async Task<IActionResult> Index(string? search, int? categoryId, string? marka, int page = 1)
    {
        var result = await _productService.SearchPagedAsync(search, categoryId, marka, isActiveFilter: true, page, PageSize);
        var columns = (await _gridColumnRepository.GetVisibleColumnsAsync())
            .OrderBy(c => c.OrderIndex)
            .ToList();

        ViewData["Search"] = search;
        ViewData["CategoryId"] = categoryId;
        ViewData["Marka"] = marka;
        ViewData["Categories"] = await _categoryRepository.GetAllAsync();
        ViewData["Brands"] = await _productService.GetDistinctBrandsAsync();
        ViewData["GridColumns"] = columns;
        ViewData["Banners"] = await _bannerService.GetActiveOrderedAsync();
        return View(result);
    }

    // GET /Home/ProductDetails/5 — ürün detay popup'ı için AJAX ile yüklenen partial view.
    public async Task<IActionResult> ProductDetails(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        return PartialView("_ProductDetails", product);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
