namespace MiniB2B.Web.Domain;

public class Order
{
    public int Id { get; set; }
    public string SiparisNo { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? KullaniciAdSoyad { get; set; } // join ile doldurulur
    public DateTime SiparisTarihi { get; set; }
    public string Durum { get; set; } = "Beklemede"; // Beklemede | Onaylandi | Reddedildi
    public decimal ToplamTutar { get; set; }
    public List<OrderItem> Kalemler { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Adet { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal ToplamFiyat { get; set; }
}
