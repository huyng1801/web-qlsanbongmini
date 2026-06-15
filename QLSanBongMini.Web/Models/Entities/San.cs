using System.ComponentModel.DataAnnotations;
using QLSanBongMini.Web.Models;

namespace QLSanBongMini.Web.Models.Entities;

public class San
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TenSan { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string LoaiSan { get; set; } = "5v5";

    [Required, MaxLength(20)]
    public string TrangThai { get; set; } = TrangThaiSan.HoatDong;

    [MaxLength(500)]
    public string? MoTa { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<BangGia> BangGias { get; set; } = new List<BangGia>();
    public ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();
}
