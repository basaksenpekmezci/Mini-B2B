namespace MiniB2B.Web.Domain;

public class Banner
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string? ResimUrl { get; set; }
    public string? Link { get; set; }
    public int Sira { get; set; }
    public bool IsActive { get; set; } = true;
}
