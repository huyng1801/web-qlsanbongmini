using System.ComponentModel.DataAnnotations;
using QLSanBongMini.Web.Models;

namespace QLSanBongMini.Web.Models.Entities;

public class ThanhToan
{
    public int Id { get; set; }

    public int DatSanId { get; set; }
    public DatSan DatSan { get; set; } = null!;

    public decimal SoTien { get; set; }

    [Required, MaxLength(20)]
    public string HinhThuc { get; set; } = HinhThucThanhToan.TienMat;

    [Required, MaxLength(20)]
    public string TrangThai { get; set; } = TrangThaiThanhToan.ChuaThanhToan;

    public DateTime? NgayThanhToan { get; set; }

    [MaxLength(300)]
    public string? GhiChu { get; set; }
}
