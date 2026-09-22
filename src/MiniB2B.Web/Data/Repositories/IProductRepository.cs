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
    /// Sipariş oluşturma sırasında birden fazla ürünün stoğunu tek transaction
    /// içinde kontrol edip düşmek için kullanılır (bkz. OrderService).
    /// </summary>
    Task<int> GetStokMiktariForUpdateAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, int productId);
    Task DecrementStokAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, int productId, int adet);
}
