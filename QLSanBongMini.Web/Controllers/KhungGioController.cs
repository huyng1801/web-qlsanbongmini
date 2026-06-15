using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class KhungGioController : Controller
{
    private readonly ApplicationDbContext _context;

    public KhungGioController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.KhungGios.KhungGioByGioBatDauAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new KhungGio());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhungGio model)
    {
        if (model.GioKetThuc <= model.GioBatDau)
        {
            ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.KhungGios.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm khung giờ.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.KhungGios.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KhungGio model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (model.GioKetThuc <= model.GioBatDau)
        {
            ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật khung giờ.";
        return RedirectToAction(nameof(Index));
    }
}
