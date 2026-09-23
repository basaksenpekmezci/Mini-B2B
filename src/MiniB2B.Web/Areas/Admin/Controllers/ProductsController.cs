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
    private const int PageSize = 20;

    private readonly IProductService _productService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductImageUploadService _imageUploadService;

    public ProductsController(IProductService productService, ICategoryRepository categoryRepository, IProductImageUploadService imageUploadService)
    {
        _productService = productService;
        _categoryRepository = categoryRepository;
        _imageUploadService = imageUploadService;
    }

    // GET /Admin/Products?search=...&categoryId=...&marka=...&durum=aktif|pasif|tumu&page=...
    // durum varsayılan "aktif": admin panelinde de öntanımlı olarak sadece aktif ürünler gösterilir,
    // ama "Pasif" veya "Tümü" seçilerek pasife alınmış ürünler de görüntülenip tekrar aktif edilebilir.
    public async Task<IActionResult> Index(string? search, int? categoryId, string? marka, string durum = "aktif", int page = 1)
    {
        bool? isActiveFilter = durum switch
        {
            "pasif" => false,
            "tumu" => null,
            _ => true
        };

        var result = await _productService.SearchPagedAsync(search, categoryId, marka, isActiveFilter, page, PageSize);

        ViewData["Search"] = search;
        ViewData["CategoryId"] = categoryId;
        ViewData["Marka"] = marka;
        ViewData["Durum"] = durum;
        ViewData["Categories"] = await _categoryRepository.GetAllAsync();
        ViewData["Brands"] = await _productService.GetDistinctBrandsAsync();
        return View(result);
    }

    // POST /Admin/Products/SetActive — ürünü pasife alır / tekrar aktif eder (soft delete yerine).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, string? returnUrl)
    {
        await _productService.SetActiveAsync(id, isActive);
        TempData["Success"] = isActive ? "Ürün tekrar aktif edildi." : "Ürün pasife alındı.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(product);
        }

        if (imageFile is not null && imageFile.Length > 0)
        {
            var upload = await _imageUploadService.UploadAsync(imageFile);
            if (!upload.Success)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage ?? "Görsel yüklenemedi.");
                await PopulateCategoriesAsync();
                return View(product);
            }

            product.ResimUrl = upload.RelativeUrl;
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
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(product);
        }

        if (imageFile is not null && imageFile.Length > 0)
        {
            var upload = await _imageUploadService.UploadAsync(imageFile);
            if (!upload.Success)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage ?? "Görsel yüklenemedi.");
                await PopulateCategoriesAsync();
                return View(product);
            }

            product.ResimUrl = upload.RelativeUrl;
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
