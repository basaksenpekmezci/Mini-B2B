using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> RegisterAsync(string ad, string soyad, string email, string? telefon, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return new RegisterResult { Success = false, ErrorMessage = "Zorunlu alanlar boş bırakılamaz." };
        }

        if (password.Length < 6)
        {
            return new RegisterResult { Success = false, ErrorMessage = "Şifre en az 6 karakter olmalıdır." };
        }

        var exists = await _userRepository.UsernameOrEmailExistsAsync(username, email);
        if (exists)
        {
            return new RegisterResult { Success = false, ErrorMessage = "Bu kullanıcı adı veya e-posta zaten kayıtlı." };
        }

        var (hash, salt) = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Ad = ad.Trim(),
            Soyad = soyad.Trim(),
            Email = email.Trim(),
            Telefon = telefon?.Trim(),
            Username = username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            IsAdmin = false
        };

        user.Id = await _userRepository.CreateAsync(user);
        return new RegisterResult { Success = true, User = user };
    }

    public async Task<User?> ValidateCredentialsAsync(string usernameOrEmail, string password)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
        if (user is null) return null;

        var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        return isValid ? user : null;
    }
}
