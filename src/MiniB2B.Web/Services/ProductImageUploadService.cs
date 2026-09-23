using Microsoft.AspNetCore.Http;

namespace MiniB2B.Web.Services;

/// <summary>
/// Admin ürün formundan yüklenen görselleri wwwroot/uploads/products altına, çakışmayan (GUID
/// tabanlı) bir isimle kaydeder. Sadece jpg/png/webp kabul edilir, en fazla 2 MB. Uzantıya ek olarak
/// dosyanın gerçek baytlarını (magic number) da kontrol ediyoruz — sadece uzantıya bakmak, kötü niyetli
/// bir dosyanın (örn. bir script) uzantısı değiştirilerek "resim" gibi yüklenmesine izin verirdi.
/// </summary>
public class ProductImageUploadService : IProductImageUploadService
{
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _env;

    public ProductImageUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<ImageUploadResult> UploadAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            return Error("Yüklenen dosya boş.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Error("Dosya boyutu 2 MB'ı aşamaz.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return Error("Sadece jpg, png veya webp formatında görsel yükleyebilirsiniz.");
        }

        await using var stream = file.OpenReadStream();
        if (!await HasValidImageSignatureAsync(stream, extension))
        {
            return Error("Dosya içeriği, uzantısıyla eşleşen geçerli bir görsel dosyası değil.");
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        stream.Position = 0;
        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream);
        }

        return new ImageUploadResult { Success = true, RelativeUrl = $"/uploads/products/{fileName}" };
    }

    private static async Task<bool> HasValidImageSignatureAsync(Stream stream, string extension)
    {
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, 12));
        if (read < 4) return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".webp" => read == 12
                        && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                        && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
            _ => false
        };
    }

    private static ImageUploadResult Error(string message) => new() { Success = false, ErrorMessage = message };
}
