using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Controllers;

[Authorize]
public class KhachHangController : Controller
{
    private readonly ApplicationDbContext _context;

    public KhachHangController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.KhachHangs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x => x.HoTen.Contains(q) || x.SoDienThoai.Contains(q));
        }

        ViewBag.Q = q;
        var items = await query.OrderByDescending(x => x.NgayTao).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new KhachHang());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhachHang model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.KhachHangs.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.KhachHangs.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KhachHang model)
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
        TempData["Success"] = "Đã cập nhật khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.KhachHangs
            .Include(x => x.DatSans)
            .ThenInclude(x => x.San)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? NotFound() : View(item);
    }
}
