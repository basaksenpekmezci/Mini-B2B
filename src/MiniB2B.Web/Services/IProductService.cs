using MiniB2B.Web.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class ProductSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IProductService
{
    Task<PagedResult<Product>> SearchPagedAsync(string? term, int? categoryId, string? marka, bool? isActiveFilter, int page, int pageSize);
    Task<IEnumerable<string>> GetDistinctBrandsAsync();

    Task<Product?> GetByIdAsync(int id);
    Task<ProductSaveResult> CreateAsync(Product product);
    Task<ProductSaveResult> UpdateAsync(Product product);
}
