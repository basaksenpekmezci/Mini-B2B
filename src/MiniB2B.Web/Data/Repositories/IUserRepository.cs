using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> UsernameOrEmailExistsAsync(string username, string email);
    Task<int> CreateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
    Task UpdateAsync(User user);
}
