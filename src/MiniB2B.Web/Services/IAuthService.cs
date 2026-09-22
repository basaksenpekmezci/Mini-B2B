using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class RegisterResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(string ad, string soyad, string email, string? telefon, string username, string password);
    Task<User?> ValidateCredentialsAsync(string usernameOrEmail, string password);
}
