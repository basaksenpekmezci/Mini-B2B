using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Web.Data.Repositories;
using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.GetAllAsync();
        return View(users);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return NotFound();
        return View(user);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id) return BadRequest();

        if (string.IsNullOrWhiteSpace(user.Ad) || string.IsNullOrWhiteSpace(user.Soyad) || string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError(string.Empty, "Ad, soyad ve e-posta zorunludur.");
            return View(user);
        }

        var existing = await _userRepository.GetByIdAsync(id);
        if (existing is null) return NotFound();

        existing.Ad = user.Ad;
        existing.Soyad = user.Soyad;
        existing.Email = user.Email;
        existing.Telefon = user.Telefon;

        await _userRepository.UpdateAsync(existing);
        TempData["Success"] = $"{existing.Ad} {existing.Soyad} bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
