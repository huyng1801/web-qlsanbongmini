using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.ViewModels;

namespace QLSanBongMini.Web.Controllers;

[Authorize]
public class LichSanController : Controller
{
    private readonly ApplicationDbContext _context;

    public LichSanController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateOnly? ngay)
    {
        var ngayXem = ngay ?? DateOnly.FromDateTime(DateTime.Today);
        var sans = await _context.Sans.OrderBy(x => x.TenSan).ToListAsync();
        var khungGios = await _context.KhungGios.KhungGioByGioBatDauAsync();

        var datSans = await _context.DatSans
            .Include(x => x.KhachHang)
            .Where(x => x.NgayDat == ngayXem && x.TrangThai != TrangThaiDatSan.DaHuy)
            .ToListAsync();

        var oDatSan = new Dictionary<(int SanId, int KhungGioId), Models.Entities.DatSan?>();
        foreach (var san in sans)
        {
            foreach (var khung in khungGios)
            {
                var dat = datSans.FirstOrDefault(x =>
                    x.SanId == san.Id
                    && x.GioBatDau < khung.GioKetThuc
                    && khung.GioBatDau < x.GioKetThuc);
                oDatSan[(san.Id, khung.Id)] = dat;
            }
        }

        var model = new LichSanViewModel
        {
            NgayXem = ngayXem,
            Sans = sans,
            KhungGios = khungGios,
            ODatSan = oDatSan
        };

        return View(model);
    }
}
