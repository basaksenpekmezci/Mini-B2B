namespace MiniB2B.Web.Data;

/// <summary>
/// Sunucu tarafı sayfalama sonucu: kayıtlar + toplam kayıt sayısı (OFFSET/FETCH ile aynı
/// round-trip'te QueryMultiple kullanılarak getirilir, bkz. ProductRepository.SearchPagedAsync).
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
