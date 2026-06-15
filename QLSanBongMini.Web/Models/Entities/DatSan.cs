using System.ComponentModel.DataAnnotations;
using QLSanBongMini.Web.Models;

namespace QLSanBongMini.Web.Models.Entities;

public class DatSan
{
    public int Id { get; set; }

    public int SanId { get; set; }
    public San San { get; set; } = null!;

    public int KhachHangId { get; set; }
    public KhachHang KhachHang { get; set; } = null!;

    public DateOnly NgayDat { get; set; }

    public TimeSpan GioBatDau { get; set; }

    public TimeSpan GioKetThuc { get; set; }

    [Required, MaxLength(20)]
    public string TrangThai { get; set; } = TrangThaiDatSan.ChoXacNhan;

    public decimal TongTien { get; set; }

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    public string? NguoiTaoId { get; set; }
    public ApplicationUser? NguoiTao { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ThanhToan? ThanhToan { get; set; }
}
