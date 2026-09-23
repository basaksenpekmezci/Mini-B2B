using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IProductGridColumnRepository
{
    /// <summary>Mağaza grid'i için: sadece görünür kolonlar.</summary>
    Task<IEnumerable<ProductGridColumn>> GetVisibleColumnsAsync();

    /// <summary>Admin "Grid Kolonları" ekranı için: görünürlüğü fark etmeksizin tüm kolonlar.</summary>
    Task<IEnumerable<ProductGridColumn>> GetAllAsync();

    Task<ProductGridColumn?> GetByIdAsync(int id);
    Task<bool> ColumnKeyExistsAsync(string columnKey, int? excludeId = null);
    Task<int> CreateAsync(ProductGridColumn column);
    Task UpdateAsync(ProductGridColumn column);
}
