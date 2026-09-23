using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _repository;

    public BannerService(IBannerRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Banner>> GetActiveOrderedAsync() => _repository.GetActiveOrderedAsync();

    public async Task<IEnumerable<Banner>> GetAllOrderedAsync() =>
        (await _repository.GetAllAsync()).OrderBy(b => b.Sira);

    public Task<Banner?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<BannerSaveResult> CreateAsync(Banner banner)
    {
        var validation = Validate(banner);
        if (validation is not null) return validation;

        await _repository.CreateAsync(banner);
        return new BannerSaveResult { Success = true };
    }

    public async Task<BannerSaveResult> UpdateAsync(Banner banner)
    {
        var validation = Validate(banner);
        if (validation is not null) return validation;

        await _repository.UpdateAsync(banner);
        return new BannerSaveResult { Success = true };
    }

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static BannerSaveResult? Validate(Banner banner)
    {
        if (string.IsNullOrWhiteSpace(banner.Baslik))
        {
            return new BannerSaveResult { Success = false, ErrorMessage = "Başlık zorunludur." };
        }

        return null;
    }
}
