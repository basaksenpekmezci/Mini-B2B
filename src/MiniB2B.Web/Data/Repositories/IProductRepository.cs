using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> SearchAsync(string? searchTerm);
    Task<Product?> GetByIdAsync(int id);
    Task<bool> UrunKoduExistsAsync(string urunKodu, int? excludeId = null);
    Task<int> CreateAsync(Product product);
    Task UpdateAsync(Product product);

    /// <summary>
    /// Sipariş oluşturma transaction'ı içinde stoğu düşmek için kullanılır (bkz. OrderService).
    /// Stok kontrolü artık ayrı bir sorguyla değil, ICartRepository.GetItemsForOrderAsync'in
    /// UPDLOCK'lu okumasından gelen değerle yapılıyor.
    /// </summary>
    Task DecrementStokAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, int productId, int adet);
}
