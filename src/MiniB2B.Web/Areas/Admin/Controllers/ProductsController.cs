using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;
using MiniB2B.Web.Services;

namespace MiniB2B.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryRepository _categoryRepository;

    public ProductsController(IProductService productService, ICategoryRepository categoryRepository)
    {
        _productService = productService;
        _categoryRepository = categoryRepository;
    }

    // GET /Admin/Products?search=...
    public async Task<IActionResult> Index(string? search)
    {
        var products = await _productService.SearchAsync(search);
        ViewData["Search"] = search;
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(product);
        }

        var result = await _productService.CreateAsync(product);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ürün kaydedilemedi.");
            await PopulateCategoriesAsync();
            return View(product);
        }

        TempData["Success"] = $"\"{product.UrunAdi}\" ürünü oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        await PopulateCategoriesAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(product);
        }

        var result = await _productService.UpdateAsync(product);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ürün güncellenemedi.");
            await PopulateCategoriesAsync();
            return View(product);
        }

        TempData["Success"] = $"\"{product.UrunAdi}\" ürünü güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await _categoryRepository.GetAllAsync();
    }
}
