namespace MiniB2B.Web.Domain;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItem> Items { get; set; } = new();

    public decimal Toplam => Items.Sum(i => i.ToplamFiyat);
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Adet { get; set; }

    // Görüntüleme için join'lenen alanlar (tabloda yok)
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public string? ResimUrl { get; set; }
    public decimal BirimFiyat { get; set; }
    public int MevcutStok { get; set; }
    public bool IsActive { get; set; } = true;

    public decimal ToplamFiyat => BirimFiyat * Adet;
}
