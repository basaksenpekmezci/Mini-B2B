using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IBannerRepository
{
    /// <summary>Ana sayfa slider'ı için: sadece aktif banner'lar, sıraya göre.</summary>
    Task<IEnumerable<Banner>> GetActiveOrderedAsync();

    /// <summary>Admin "Banner'lar" ekranı için: aktiflik fark etmeksizin tümü.</summary>
    Task<IEnumerable<Banner>> GetAllAsync();

    Task<Banner?> GetByIdAsync(int id);
    Task<int> CreateAsync(Banner banner);
    Task UpdateAsync(Banner banner);
    Task DeleteAsync(int id);
}
