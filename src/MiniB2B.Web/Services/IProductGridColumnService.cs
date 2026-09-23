using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class GridColumnSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IProductGridColumnService
{
    Task<IEnumerable<ProductGridColumn>> GetAllOrderedAsync();
    Task<ProductGridColumn?> GetByIdAsync(int id);
    Task<GridColumnSaveResult> CreateAsync(ProductGridColumn column);
    Task<GridColumnSaveResult> UpdateAsync(ProductGridColumn column);
}
