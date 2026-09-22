using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
    [Display(Name = "Kullanıcı Adı / E-posta")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
