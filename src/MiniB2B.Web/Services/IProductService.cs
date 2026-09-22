using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class ProductSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IProductService
{
    Task<IEnumerable<Product>> SearchAsync(string? term);
    Task<Product?> GetByIdAsync(int id);
    Task<ProductSaveResult> CreateAsync(Product product);
    Task<ProductSaveResult> UpdateAsync(Product product);
}
