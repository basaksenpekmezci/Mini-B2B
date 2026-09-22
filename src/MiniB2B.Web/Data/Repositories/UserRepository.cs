using Dapper;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Ad, Soyad, Email, Telefon, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAt
            FROM dbo.Users
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        const string sql = @"
            SELECT Id, Ad, Soyad, Email, Telefon, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAt
            FROM dbo.Users
            WHERE Username = @Value OR Email = @Value;";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Value = usernameOrEmail });
    }

    public async Task<bool> UsernameOrEmailExistsAsync(string username, string email)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.Users
            WHERE Username = @Username OR Email = @Email;";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username, Email = email });
        return count > 0;
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO dbo.Users (Ad, Soyad, Email, Telefon, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Ad, @Soyad, @Email, @Telefon, @Username, @PasswordHash, @PasswordSalt, @IsAdmin, SYSUTCDATETIME());";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Ad, Soyad, Email, Telefon, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAt
            FROM dbo.Users
            ORDER BY CreatedAt DESC;";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(sql);
    }

    public async Task UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE dbo.Users
            SET Ad = @Ad,
                Soyad = @Soyad,
                Email = @Email,
                Telefon = @Telefon
            WHERE Id = @Id;";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }
}
