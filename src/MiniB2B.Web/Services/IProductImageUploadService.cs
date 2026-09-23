using Microsoft.AspNetCore.Http;

namespace MiniB2B.Web.Services;

public class ImageUploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RelativeUrl { get; set; }
}

public interface IProductImageUploadService
{
    Task<ImageUploadResult> UploadAsync(IFormFile file);
}
