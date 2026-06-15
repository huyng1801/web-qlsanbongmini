using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Controllers;

[Authorize]
public class SanController : Controller
{
    private readonly ApplicationDbContext _context;

    public SanController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sans = await _context.Sans.OrderBy(x => x.TenSan).ToListAsync();
        return View(sans);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Create()
    {
        return View(new San());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(San model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Sans.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm sân mới.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var san = await _context.Sans.FindAsync(id);
        if (san == null)
        {
            return NotFound();
        }

        return View(san);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id, San model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật sân.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var san = await _context.Sans.FindAsync(id);
        if (san == null)
        {
            return NotFound();
        }

        return View(san);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var san = await _context.Sans.FindAsync(id);
        if (san != null)
        {
            var coDatSan = await _context.DatSans.AnyAsync(x => x.SanId == id && x.TrangThai != TrangThaiDatSan.DaHuy);
            if (coDatSan)
            {
                TempData["Error"] = "Không thể xóa sân đang có lịch đặt.";
                return RedirectToAction(nameof(Index));
            }

            _context.Sans.Remove(san);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sân.";
        }

        return RedirectToAction(nameof(Index));
    }
}
