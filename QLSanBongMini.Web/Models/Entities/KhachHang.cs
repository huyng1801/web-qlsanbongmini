using System.ComponentModel.DataAnnotations;

namespace QLSanBongMini.Web.Models.Entities;

public class KhachHang
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, MaxLength(15)]
    public string SoDienThoai { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();
}
