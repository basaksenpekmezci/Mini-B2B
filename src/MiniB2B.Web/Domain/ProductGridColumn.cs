namespace MiniB2B.Web.Domain;

public enum GridRenderType
{
    Text,
    Image,
    Number,
    Currency,
    StockBadge,
    QtyInput
}

public class ProductGridColumn
{
    public int Id { get; set; }
    public string ColumnKey { get; set; } = string.Empty; // Product property adı veya sanal alan (StokDurumu, AdetGiris)
    public string DisplayName { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string RenderType { get; set; } = "Text";
    public bool IsVisible { get; set; } = true;
    public bool ShowOnDesktop { get; set; } = true;
    public bool ShowOnTablet { get; set; } = true;
    public bool ShowOnMobile { get; set; } = true;
    public string? Width { get; set; }
    public string Alignment { get; set; } = "left";

    public GridRenderType RenderTypeEnum =>
        Enum.TryParse<GridRenderType>(RenderType, out var t) ? t : GridRenderType.Text;
}
