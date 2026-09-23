using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class ProductGridColumnService : IProductGridColumnService
{
    private static readonly string[] AllowedAlignments = { "left", "center", "right" };

    private readonly IProductGridColumnRepository _repository;

    public ProductGridColumnService(IProductGridColumnRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductGridColumn>> GetAllOrderedAsync() =>
        (await _repository.GetAllAsync()).OrderBy(c => c.OrderIndex);

    public Task<ProductGridColumn?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<GridColumnSaveResult> CreateAsync(ProductGridColumn column)
    {
        var validation = Validate(column);
        if (validation is not null) return validation;

        if (await _repository.ColumnKeyExistsAsync(column.ColumnKey))
        {
            return new GridColumnSaveResult { Success = false, ErrorMessage = $"\"{column.ColumnKey}\" kolon anahtarı zaten kullanılıyor." };
        }

        await _repository.CreateAsync(column);
        return new GridColumnSaveResult { Success = true };
    }

    public async Task<GridColumnSaveResult> UpdateAsync(ProductGridColumn column)
    {
        var validation = Validate(column);
        if (validation is not null) return validation;

        if (await _repository.ColumnKeyExistsAsync(column.ColumnKey, column.Id))
        {
            return new GridColumnSaveResult { Success = false, ErrorMessage = $"\"{column.ColumnKey}\" kolon anahtarı başka bir kolonda kullanılıyor." };
        }

        await _repository.UpdateAsync(column);
        return new GridColumnSaveResult { Success = true };
    }

    /// <summary>
    /// ColumnKey, Product sınıfında gerçekten var olan bir property adı olmalıdır (reflection ile
    /// ProductGridRenderer üzerinden render edilebilmesi için) — "QtyInput" render tipi bunun tek
    /// istisnasıdır, çünkü o kolon Product üzerinde bir alan değil, adet girişi + sepete ekle
    /// aksiyonunu temsil eden sanal bir kolondur (bkz. Views/Home/Index.cshtml).
    /// </summary>
    private static GridColumnSaveResult? Validate(ProductGridColumn column)
    {
        if (string.IsNullOrWhiteSpace(column.ColumnKey) || string.IsNullOrWhiteSpace(column.DisplayName))
        {
            return Error("Kolon anahtarı (ColumnKey) ve görünen ad (DisplayName) zorunludur.");
        }

        if (!Enum.TryParse<GridRenderType>(column.RenderType, out var renderType))
        {
            var allowed = string.Join(", ", Enum.GetNames<GridRenderType>());
            return Error($"Geçersiz render tipi: \"{column.RenderType}\". Geçerli değerler: {allowed}.");
        }

        if (!AllowedAlignments.Contains(column.Alignment))
        {
            return Error($"Geçersiz hizalama: \"{column.Alignment}\". Geçerli değerler: {string.Join(", ", AllowedAlignments)}.");
        }

        if (renderType != GridRenderType.QtyInput && !ProductGridRenderer.ProductHasProperty(column.ColumnKey))
        {
            return Error($"\"{column.ColumnKey}\" Product sınıfında bir property olarak bulunamadı. " +
                          "ColumnKey, Product üzerindeki gerçek bir property adıyla birebir eşleşmelidir " +
                          "(örn. UrunAdi, Marka, Fiyat, StokDurumu) — bu kural yalnızca \"QtyInput\" render " +
                          "tipinde (adet girişi + sepete ekle, Product üzerinde karşılığı olmayan sanal bir kolon) uygulanmaz.");
        }

        return null;

        static GridColumnSaveResult Error(string message) => new() { Success = false, ErrorMessage = message };
    }
}
