using MiniB2B.Web.Data;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<PagedResult<Product>> SearchPagedAsync(string? term, int? categoryId, string? marka, bool? isActiveFilter, int page, int pageSize) =>
        _productRepository.SearchPagedAsync(term, categoryId, marka, isActiveFilter, page, pageSize);

    public Task<IEnumerable<string>> GetDistinctBrandsAsync() => _productRepository.GetDistinctBrandsAsync();

    public Task<Product?> GetByIdAsync(int id) => _productRepository.GetByIdAsync(id);

    public async Task<ProductSaveResult> CreateAsync(Product product)
    {
        var validation = Validate(product);
        if (validation is not null) return validation;

        if (await _productRepository.UrunKoduExistsAsync(product.UrunKodu))
        {
            return new ProductSaveResult { Success = false, ErrorMessage = "Bu ürün kodu zaten kullanılıyor." };
        }

        await _productRepository.CreateAsync(product);
        return new ProductSaveResult { Success = true };
    }

    public async Task<ProductSaveResult> UpdateAsync(Product product)
    {
        var validation = Validate(product);
        if (validation is not null) return validation;

        if (await _productRepository.UrunKoduExistsAsync(product.UrunKodu, product.Id))
        {
            return new ProductSaveResult { Success = false, ErrorMessage = "Bu ürün kodu başka bir üründe kullanılıyor." };
        }

        await _productRepository.UpdateAsync(product);
        return new ProductSaveResult { Success = true };
    }

    private static ProductSaveResult? Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.UrunKodu) || string.IsNullOrWhiteSpace(product.UrunAdi))
        {
            return new ProductSaveResult { Success = false, ErrorMessage = "Ürün kodu ve ürün adı zorunludur." };
        }

        if (product.Fiyat < 0)
        {
            return new ProductSaveResult { Success = false, ErrorMessage = "Fiyat negatif olamaz." };
        }

        if (product.StokMiktari < 0)
        {
            return new ProductSaveResult { Success = false, ErrorMessage = "Stok miktarı negatif olamaz." };
        }

        return null;
    }
}
