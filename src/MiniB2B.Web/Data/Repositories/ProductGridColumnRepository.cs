using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class ProductGridColumnRepository : IProductGridColumnRepository
{
    private readonly DapperContext _context;

    public ProductGridColumnRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductGridColumn>> GetVisibleColumnsAsync()
    {
        const string sql = @"
            SELECT Id, ColumnKey, DisplayName, OrderIndex, RenderType,
                   IsVisible, ShowOnDesktop, ShowOnTablet, ShowOnMobile, Width, Alignment
            FROM dbo.ProductGridColumns
            WHERE IsVisible = 1
            ORDER BY OrderIndex;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProductGridColumn>(sql);
    }

    public async Task<IEnumerable<ProductGridColumn>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, ColumnKey, DisplayName, OrderIndex, RenderType,
                   IsVisible, ShowOnDesktop, ShowOnTablet, ShowOnMobile, Width, Alignment
            FROM dbo.ProductGridColumns
            ORDER BY OrderIndex;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProductGridColumn>(sql);
    }

    public async Task<ProductGridColumn?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, ColumnKey, DisplayName, OrderIndex, RenderType,
                   IsVisible, ShowOnDesktop, ShowOnTablet, ShowOnMobile, Width, Alignment
            FROM dbo.ProductGridColumns
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProductGridColumn>(sql, new { Id = id });
    }

    public async Task<bool> ColumnKeyExistsAsync(string columnKey, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1) FROM dbo.ProductGridColumns
            WHERE ColumnKey = @ColumnKey AND (@ExcludeId IS NULL OR Id <> @ExcludeId);";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ColumnKey = columnKey, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<int> CreateAsync(ProductGridColumn column)
    {
        const string sql = @"
            INSERT INTO dbo.ProductGridColumns
                (ColumnKey, DisplayName, OrderIndex, RenderType, IsVisible, ShowOnDesktop, ShowOnTablet, ShowOnMobile, Width, Alignment)
            OUTPUT INSERTED.Id
            VALUES
                (@ColumnKey, @DisplayName, @OrderIndex, @RenderType, @IsVisible, @ShowOnDesktop, @ShowOnTablet, @ShowOnMobile, @Width, @Alignment);";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, column);
    }

    public async Task UpdateAsync(ProductGridColumn column)
    {
        const string sql = @"
            UPDATE dbo.ProductGridColumns
            SET ColumnKey = @ColumnKey,
                DisplayName = @DisplayName,
                OrderIndex = @OrderIndex,
                RenderType = @RenderType,
                IsVisible = @IsVisible,
                ShowOnDesktop = @ShowOnDesktop,
                ShowOnTablet = @ShowOnTablet,
                ShowOnMobile = @ShowOnMobile,
                Width = @Width,
                Alignment = @Alignment
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, column);
    }
}
