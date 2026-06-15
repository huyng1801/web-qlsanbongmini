using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.ViewModels;

namespace QLSanBongMini.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class NguoiDungController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public NguoiDungController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(x => x.UserName).ToListAsync();
        var model = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var user in users)
        {
            model.Add((user, await _userManager.GetRolesAsync(user)));
        }

        return View(model);
    }

    public IActionResult Create()
    {
        return View(new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Vui lòng nhập mật khẩu.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email ?? $"{model.UserName}@qlsanbong.local",
            EmailConfirmed = true,
            HoTen = model.HoTen,
            DangHoatDong = model.DangHoatDong
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        TempData["Success"] = "Đã tạo tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserFormViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            HoTen = user.HoTen,
            Email = user.Email,
            Role = roles.FirstOrDefault() ?? AppRoles.NhanVien,
            DangHoatDong = user.DangHoatDong
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id!);
        if (user == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.HoTen = model.HoTen;
        user.Email = model.Email;
        user.DangHoatDong = model.DangHoatDong;
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, model.Password);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Index));
    }
}
