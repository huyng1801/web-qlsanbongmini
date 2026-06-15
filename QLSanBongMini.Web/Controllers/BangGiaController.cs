using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class BangGiaController : Controller
{
    private readonly ApplicationDbContext _context;

    public BangGiaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = (await _context.BangGias
            .Include(x => x.San)
            .Include(x => x.KhungGio)
            .ToListAsync())
            .OrderBy(x => x.San.TenSan)
            .ThenBy(x => x.KhungGio.GioBatDau)
            .ThenBy(x => x.LaCuoiTuan)
            .ToList();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectListsAsync();
        return View(new BangGia());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BangGia model)
    {
        var exists = await _context.BangGias.AnyAsync(x =>
            x.SanId == model.SanId && x.KhungGioId == model.KhungGioId && x.LaCuoiTuan == model.LaCuoiTuan);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "Bảng giá cho sân và khung giờ này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(model);
        }

        _context.BangGias.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm bảng giá.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.BangGias.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        await LoadSelectListsAsync();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BangGia model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật bảng giá.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.SanId = new SelectList(await _context.Sans.OrderBy(x => x.TenSan).ToListAsync(), "Id", "TenSan");
        ViewBag.KhungGioId = new SelectList(await _context.KhungGios.KhungGioByGioBatDauAsync(), "Id", "TenKhung");
    }
}
