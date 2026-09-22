namespace MiniB2B.Web.Domain;

public class Product
{
    public int Id { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? Marka { get; set; }
    public string? UreticiKodu { get; set; }
    public string? OzelKod1 { get; set; }
    public string? OzelKod2 { get; set; }
    public string? ResimUrl { get; set; }
    public int StokMiktari { get; set; }
    public int KritikStokSeviyesi { get; set; } = 5;
    public decimal Fiyat { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryAdi { get; set; } // join ile doldurulur, tabloda yok
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Stok göstergesi için türetilmiş alan (DB'de tutulmaz).
    /// Dinamik grid'de "StockBadge" render tipi bunu kullanır.
    /// </summary>
    public string StokDurumu =>
        StokMiktari <= 0 ? "Yok" :
        StokMiktari < KritikStokSeviyesi ? "Kritik" : "Var";
}
