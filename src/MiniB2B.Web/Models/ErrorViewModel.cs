namespace MiniB2B.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Kullanıcıya gösterilecek, teknik detay (exception mesajı/stack trace) içermeyen genel mesaj.
    /// Durum koduna göre biraz daha spesifik ama yine de güvenli bir metin döner.
    /// </summary>
    public string Message => StatusCode switch
    {
        404 => "Aradığınız sayfa bulunamadı.",
        403 => "Bu sayfaya erişim yetkiniz yok.",
        _ => "Beklenmeyen bir hata oluştu. Sorun devam ederse lütfen yukarıdaki istek numarasıyla bize ulaşın."
    };
}
