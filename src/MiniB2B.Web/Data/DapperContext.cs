using System.Data;
using Microsoft.Data.SqlClient;

namespace MiniB2B.Web.Data;

/// <summary>
/// Her repository çağrısında yeni, kısa ömürlü bir SqlConnection üretir.
/// Dapper connection pooling'i .NET/ADO.NET seviyesinde zaten yönetir,
/// bu yüzden burada connection'ı elle havuzlamaya gerek yok.
/// </summary>
public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection bağlantı dizesi appsettings.json içinde bulunamadı.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
