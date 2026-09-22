using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Models;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductGridColumnRepository _gridColumnRepository;

    public HomeController(IProductService productService, IProductGridColumnRepository gridColumnRepository)
    {
        _productService = productService;
        _gridColumnRepository = gridColumnRepository;
    }

    // GET /  veya  /Home/Index?search=...
    // Dinamik ürün grid'i: kolonlar (hangi alan, sıra, render tipi, cihaz görünürlüğü) veritabanından
    // (ProductGridColumns) okunur; her hücrenin değeri ProductGridRenderer ile reflection üzerinden çözülür.
    public async Task<IActionResult> Index(string? search)
    {
        var products = await _productService.SearchAsync(search);
        var columns = (await _gridColumnRepository.GetVisibleColumnsAsync())
            .OrderBy(c => c.OrderIndex)
            .ToList();

        ViewData["Search"] = search;
        ViewData["GridColumns"] = columns;
        return View(products);
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
