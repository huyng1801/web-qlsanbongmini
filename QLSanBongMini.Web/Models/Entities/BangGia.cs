using System.ComponentModel.DataAnnotations;

namespace QLSanBongMini.Web.Models.Entities;

public class BangGia
{
    public int Id { get; set; }

    public int SanId { get; set; }
    public San San { get; set; } = null!;

    public int KhungGioId { get; set; }
    public KhungGio KhungGio { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Gia { get; set; }

    public bool LaCuoiTuan { get; set; }
}
