using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DapperContext _context;

    public CategoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        const string sql = "SELECT Id, Ad FROM dbo.Categories ORDER BY Ad;";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Category>(sql);
    }
}
