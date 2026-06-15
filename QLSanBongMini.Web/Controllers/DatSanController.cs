using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;
using QLSanBongMini.Web.Models.ViewModels;
using QLSanBongMini.Web.Services;

namespace QLSanBongMini.Web.Controllers;

[Authorize]
public class DatSanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDatSanService _datSanService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatSanController(
        ApplicationDbContext context,
        IDatSanService datSanService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _datSanService = datSanService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(DateOnly? ngay, string? trangThai)
    {
        var query = _context.DatSans
            .Include(x => x.San)
            .Include(x => x.KhachHang)
            .Include(x => x.ThanhToan)
            .AsQueryable();

        if (ngay.HasValue)
        {
            query = query.Where(x => x.NgayDat == ngay.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(x => x.TrangThai == trangThai);
        }

        ViewBag.Ngay = ngay?.ToString("yyyy-MM-dd");
        ViewBag.TrangThai = trangThai;
        ViewBag.TrangThaiList = TrangThaiDatSan.Labels;

        var items = (await query.ToListAsync()).DatSanByNgayGioAsc();
        return View(items);
    }

    public async Task<IActionResult> Create(int? sanId, DateOnly? ngay, int? khungGioId)
    {
        var model = new DatSanFormViewModel();
        if (sanId.HasValue)
        {
            model.SanId = sanId.Value;
        }

        if (ngay.HasValue)
        {
            model.NgayDat = ngay.Value;
        }

        if (khungGioId.HasValue)
        {
            var khung = await _context.KhungGios.FindAsync(khungGioId.Value);
            if (khung != null)
            {
                model.GioBatDau = khung.GioBatDau;
                model.GioKetThuc = khung.GioBatDau.Add(TimeSpan.FromMinutes(90));
                if (model.GioKetThuc > khung.GioKetThuc)
                {
                    model.GioKetThuc = khung.GioKetThuc;
                }
            }
        }

        model.TongTien = await _datSanService.TinhTienAsync(model.SanId, model.NgayDat, model.GioBatDau, model.GioKetThuc);
        await LoadFormListsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DatSanFormViewModel model)
    {
        if (model.GioKetThuc <= model.GioBatDau)
        {
            ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
        }

        var san = await _context.Sans.FindAsync(model.SanId);
        if (san?.TrangThai == TrangThaiSan.BaoTri)
        {
            ModelState.AddModelError(nameof(model.SanId), "Sân đang bảo trì, không thể đặt.");
        }

        if (await _datSanService.CoTrungLichAsync(model.SanId, model.NgayDat, model.GioBatDau, model.GioKetThuc))
        {
            ModelState.AddModelError(string.Empty, "Khung giờ này đã có lịch đặt.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormListsAsync(model);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var tongTien = await _datSanService.TinhTienAsync(model.SanId, model.NgayDat, model.GioBatDau, model.GioKetThuc);

        var datSan = new DatSan
        {
            SanId = model.SanId,
            KhachHangId = model.KhachHangId,
            NgayDat = model.NgayDat,
            GioBatDau = model.GioBatDau,
            GioKetThuc = model.GioKetThuc,
            GhiChu = model.GhiChu,
            TongTien = tongTien,
            TrangThai = TrangThaiDatSan.ChoXacNhan,
            NguoiTaoId = user?.Id
        };

        _context.DatSans.Add(datSan);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã tạo lịch đặt sân.";
        return RedirectToAction(nameof(Details), new { id = datSan.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.DatSans
            .Include(x => x.San)
            .Include(x => x.KhachHang)
            .Include(x => x.ThanhToan)
            .Include(x => x.NguoiTao)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan(int id)
    {
        var item = await _context.DatSans.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        item.TrangThai = TrangThaiDatSan.DaXacNhan;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xác nhận lịch đặt.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var item = await _context.DatSans.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        item.TrangThai = TrangThaiDatSan.DaHuy;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã hủy lịch đặt.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ThanhToan(int id)
    {
        var datSan = await _context.DatSans
            .Include(x => x.ThanhToan)
            .Include(x => x.KhachHang)
            .Include(x => x.San)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (datSan == null)
        {
            return NotFound();
        }

        var model = new ThanhToanFormViewModel
        {
            DatSanId = id,
            TongTienDatSan = datSan.TongTien,
            SoTien = datSan.ThanhToan?.SoTien ?? datSan.TongTien,
            HinhThuc = datSan.ThanhToan?.HinhThuc ?? HinhThucThanhToan.TienMat,
            TrangThai = datSan.ThanhToan?.TrangThai ?? TrangThaiThanhToan.DaThanhToanDu,
            GhiChu = datSan.ThanhToan?.GhiChu
        };

        ViewBag.DatSan = datSan;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThanhToan(ThanhToanFormViewModel model)
    {
        var datSan = await _context.DatSans
            .Include(x => x.ThanhToan)
            .FirstOrDefaultAsync(x => x.Id == model.DatSanId);
        if (datSan == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DatSan = datSan;
            return View(model);
        }

        if (datSan.ThanhToan == null)
        {
            datSan.ThanhToan = new ThanhToan { DatSanId = datSan.Id };
            _context.ThanhToans.Add(datSan.ThanhToan);
        }

        datSan.ThanhToan.SoTien = model.SoTien;
        datSan.ThanhToan.HinhThuc = model.HinhThuc;
        datSan.ThanhToan.TrangThai = model.TrangThai;
        datSan.ThanhToan.GhiChu = model.GhiChu;
        datSan.ThanhToan.NgayThanhToan = DateTime.Now;

        if (model.TrangThai == TrangThaiThanhToan.DaThanhToanDu)
        {
            datSan.TrangThai = TrangThaiDatSan.DaThanhToan;
        }
        else if (datSan.TrangThai == TrangThaiDatSan.ChoXacNhan)
        {
            datSan.TrangThai = TrangThaiDatSan.DaXacNhan;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thanh toán.";
        return RedirectToAction(nameof(Details), new { id = datSan.Id });
    }

    [HttpGet]
    public async Task<IActionResult> TinhTien(int sanId, DateOnly ngayDat, string gioBatDau, string gioKetThuc)
    {
        if (!TimeSpan.TryParse(gioBatDau, out var batDau) || !TimeSpan.TryParse(gioKetThuc, out var ketThuc))
        {
            return BadRequest();
        }

        var tien = await _datSanService.TinhTienAsync(sanId, ngayDat, batDau, ketThuc);
        return Json(new { tongTien = tien });
    }

    private async Task LoadFormListsAsync(DatSanFormViewModel model)
    {
        ViewBag.SanId = new SelectList(
            await _context.Sans.Where(x => x.TrangThai == TrangThaiSan.HoatDong).OrderBy(x => x.TenSan).ToListAsync(),
            "Id", "TenSan", model.SanId);
        ViewBag.KhachHangId = new SelectList(
            await _context.KhachHangs.OrderBy(x => x.HoTen).ToListAsync(),
            "Id", "HoTen", model.KhachHangId);
    }
}
