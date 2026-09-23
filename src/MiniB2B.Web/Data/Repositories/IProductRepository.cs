using MiniB2B.Web.Data;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IProductRepository
{
    /// <summary>
    /// Metin arama + kategori/marka filtresi + sunucu taraflı sayfalama (OFFSET/FETCH), tümü SQL
    /// tarafında; sayfa kayıtları ve toplam kayıt sayısı tek round-trip'te (QueryMultiple) gelir.
    /// isActiveFilter: true = sadece aktif (mağaza), false = sadece pasif, null = tümü (admin "Tümü").
    /// </summary>
    Task<PagedResult<Product>> SearchPagedAsync(string? searchTerm, int? categoryId, string? marka, bool? isActiveFilter, int page, int pageSize);

    /// <summary>Filtre dropdown'ı için, ürünlerde kullanılan benzersiz marka listesi.</summary>
    Task<IEnumerable<string>> GetDistinctBrandsAsync();

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
