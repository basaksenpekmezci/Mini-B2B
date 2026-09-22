namespace MiniB2B.Web.Services;

/// <summary>
/// Sipariş durumu (Beklemede/Onaylandi/Reddedildi) için görsel gösterge (renk) ve Türkçe etiket eşlemesi.
/// Durum değerleri veritabanında CK_Orders_Durum kısıtına uyacak şekilde ASCII tutulur (örn. "Onaylandi");
/// ekranda gösterilecek Türkçe etiket burada üretilir.
/// </summary>
public static class OrderStatusBadge
{
    public static string CssClass(string durum) => durum switch
    {
        "Onaylandi" => "bg-success",
        "Reddedildi" => "bg-danger",
        _ => "bg-secondary"
    };

    public static string Label(string durum) => durum switch
    {
        "Onaylandi" => "Onaylandı",
        "Reddedildi" => "Reddedildi",
        _ => "Beklemede"
    };
}
