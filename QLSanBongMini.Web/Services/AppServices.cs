using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Data;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Services;

public interface IDatSanService
{
    Task<bool> CoTrungLichAsync(int sanId, DateOnly ngayDat, TimeSpan gioBatDau, TimeSpan gioKetThuc, int? excludeId = null);
    Task<decimal> TinhTienAsync(int sanId, DateOnly ngayDat, TimeSpan gioBatDau, TimeSpan gioKetThuc);
    Task<List<KhungGio>> LayKhungGioKhaDungAsync();
}

public class DatSanService : IDatSanService
{
    private readonly ApplicationDbContext _context;

    public DatSanService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CoTrungLichAsync(int sanId, DateOnly ngayDat, TimeSpan gioBatDau, TimeSpan gioKetThuc, int? excludeId = null)
    {
        var query = _context.DatSans
            .Where(x => x.SanId == sanId
                        && x.NgayDat == ngayDat
                        && x.TrangThai != TrangThaiDatSan.DaHuy
                        && x.GioBatDau < gioKetThuc
                        && gioBatDau < x.GioKetThuc);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<decimal> TinhTienAsync(int sanId, DateOnly ngayDat, TimeSpan gioBatDau, TimeSpan gioKetThuc)
    {
        var laCuoiTuan = ngayDat.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var khungGios = await _context.KhungGios.KhungGioByGioBatDauAsync();
        var bangGias = await _context.BangGias
            .Where(x => x.SanId == sanId && x.LaCuoiTuan == laCuoiTuan)
            .Include(x => x.KhungGio)
            .ToListAsync();

        decimal tong = 0;
        var thoiDiem = gioBatDau;

        while (thoiDiem < gioKetThuc)
        {
            var khung = khungGios.FirstOrDefault(k => thoiDiem >= k.GioBatDau && thoiDiem < k.GioKetThuc);
            if (khung == null)
            {
                break;
            }

            var ketThucKhung = khung.GioKetThuc < gioKetThuc ? khung.GioKetThuc : gioKetThuc;
            var soGio = (decimal)(ketThucKhung - thoiDiem).TotalHours;
            var gia = bangGias.FirstOrDefault(x => x.KhungGioId == khung.Id)?.Gia ?? 0;
            var giaMotGio = gia / (decimal)(khung.GioKetThuc - khung.GioBatDau).TotalHours;
            tong += giaMotGio * soGio;
            thoiDiem = ketThucKhung;
        }

        return Math.Round(tong, 0);
    }

    public Task<List<KhungGio>> LayKhungGioKhaDungAsync() =>
        _context.KhungGios.KhungGioByGioBatDauAsync();
}

public interface IBaoCaoService
{
    Task<DashboardViewModel> LayThongKeAsync();
}

public class DashboardViewModel
{
    public int TongSan { get; set; }
    public int LichHomNay { get; set; }
    public int LichThangNay { get; set; }
    public decimal DoanhThuThang { get; set; }
    public List<SanThongKe> SanDuocDatNhieu { get; set; } = new();
    public List<DatSan> DatSanGanDay { get; set; } = new();
}

public class SanThongKe
{
    public string TenSan { get; set; } = string.Empty;
    public int SoLuot { get; set; }
}

public class BaoCaoService : IBaoCaoService
{
    private readonly ApplicationDbContext _context;

    public BaoCaoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> LayThongKeAsync()
    {
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        var dauThang = new DateOnly(homNay.Year, homNay.Month, 1);
        var cuoiThang = dauThang.AddMonths(1).AddDays(-1);

        var datSans = await _context.DatSans
            .Include(x => x.San)
            .Include(x => x.KhachHang)
            .Where(x => x.TrangThai != TrangThaiDatSan.DaHuy)
            .ToListAsync();

        var datSansThang = datSans.Where(x => x.NgayDat >= dauThang && x.NgayDat <= cuoiThang).ToList();

        var thanhToans = await _context.ThanhToans
            .Include(x => x.DatSan)
            .Where(x => x.DatSan.TrangThai != TrangThaiDatSan.DaHuy
                        && x.DatSan.NgayDat >= dauThang
                        && x.DatSan.NgayDat <= cuoiThang)
            .ToListAsync();

        return new DashboardViewModel
        {
            TongSan = await _context.Sans.CountAsync(x => x.TrangThai == TrangThaiSan.HoatDong),
            LichHomNay = datSans.Count(x => x.NgayDat == homNay),
            LichThangNay = datSansThang.Count,
            DoanhThuThang = thanhToans.Sum(x => x.SoTien),
            SanDuocDatNhieu = datSansThang
                .GroupBy(x => x.San.TenSan)
                .Select(g => new SanThongKe { TenSan = g.Key, SoLuot = g.Count() })
                .OrderByDescending(x => x.SoLuot)
                .Take(5)
                .ToList(),
            DatSanGanDay = datSans.DatSanByNgayGioDesc().Take(8).ToList()
        };
    }
}
