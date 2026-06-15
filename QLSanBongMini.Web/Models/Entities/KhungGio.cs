using System.ComponentModel.DataAnnotations;

namespace QLSanBongMini.Web.Models.Entities;

public class KhungGio
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string TenKhung { get; set; } = string.Empty;

    public TimeSpan GioBatDau { get; set; }

    public TimeSpan GioKetThuc { get; set; }

    public ICollection<BangGia> BangGias { get; set; } = new List<BangGia>();
}
