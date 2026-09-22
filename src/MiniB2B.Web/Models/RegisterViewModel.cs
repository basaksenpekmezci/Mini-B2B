using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [Display(Name = "Soyad")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre (Tekrar)")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    public string PasswordConfirm { get; set; } = string.Empty;
}
