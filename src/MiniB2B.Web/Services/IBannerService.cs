using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class BannerSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IBannerService
{
    Task<IEnumerable<Banner>> GetActiveOrderedAsync();
    Task<IEnumerable<Banner>> GetAllOrderedAsync();
    Task<Banner?> GetByIdAsync(int id);
    Task<BannerSaveResult> CreateAsync(Banner banner);
    Task<BannerSaveResult> UpdateAsync(Banner banner);
    Task DeleteAsync(int id);
}
