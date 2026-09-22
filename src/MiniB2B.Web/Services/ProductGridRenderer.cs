using System.Reflection;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

/// <summary>
/// Dinamik ürün grid'i için kolon->değer çözümlemesi. Kolon listesi (hangi alan, hangi sırada,
/// hangi render tipiyle) veritabanındaki ProductGridColumns tablosundan gelir; burada kod
/// değiştirmeden, ColumnKey'e karşılık gelen Product property'sine reflection ile erişilir.
/// "AdetGiris" gibi Product üzerinde karşılığı olmayan sanal kolonlar (adet girişi + sepete ekle
/// aksiyonu) view tarafında RenderType == QtyInput özel durumu olarak ele alınır.
/// </summary>
public static class ProductGridRenderer
{
    private static readonly Dictionary<string, PropertyInfo?> PropertyCache = new();

    public static object? GetValue(Product product, string columnKey)
    {
        if (!PropertyCache.TryGetValue(columnKey, out var prop))
        {
            prop = typeof(Product).GetProperty(columnKey, BindingFlags.Public | BindingFlags.Instance);
            PropertyCache[columnKey] = prop;
        }

        return prop?.GetValue(product);
    }

    /// <summary>
    /// Kolonun ShowOnDesktop/Tablet/Mobile bayraklarına göre Bootstrap responsive görünürlük sınıfı üretir.
    /// </summary>
    public static string ResponsiveClass(ProductGridColumn column)
    {
        if (!column.ShowOnDesktop && !column.ShowOnTablet && !column.ShowOnMobile) return "d-none";
        if (column.ShowOnMobile) return string.Empty;
        if (column.ShowOnTablet) return "d-none d-md-table-cell";
        return "d-none d-lg-table-cell";
    }

    public static string StockBadgeClass(string stokDurumu) => stokDurumu switch
    {
        "Var" => "badge-stok-var",
        "Kritik" => "badge-stok-kritik",
        "Yok" => "badge-stok-yok",
        _ => "bg-secondary"
    };
}
