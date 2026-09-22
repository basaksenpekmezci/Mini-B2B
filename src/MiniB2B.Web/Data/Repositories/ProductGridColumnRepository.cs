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
}
