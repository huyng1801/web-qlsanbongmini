using System.ComponentModel.DataAnnotations;

namespace QLSanBongMini.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required, Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required, Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [EmailAddress, Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Mật khẩu")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required, Display(Name = "Vai trò")]
    public string Role { get; set; } = AppRoles.NhanVien;

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;
}

public class DatSanFormViewModel
{
    public int? Id { get; set; }

    [Required, Display(Name = "Sân")]
    public int SanId { get; set; }

    [Required, Display(Name = "Khách hàng")]
    public int KhachHangId { get; set; }

    [Required, Display(Name = "Ngày đặt")]
    [DataType(DataType.Date)]
    public DateOnly NgayDat { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, Display(Name = "Giờ bắt đầu")]
    public TimeSpan GioBatDau { get; set; } = new(18, 0, 0);

    [Required, Display(Name = "Giờ kết thúc")]
    public TimeSpan GioKetThuc { get; set; } = new(19, 30, 0);

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    [Display(Name = "Tổng tiền")]
    public decimal TongTien { get; set; }
}

public class ThanhToanFormViewModel
{
    public int DatSanId { get; set; }

    [Display(Name = "Tổng tiền đặt sân")]
    public decimal TongTienDatSan { get; set; }

    [Required, Display(Name = "Số tiền thanh toán")]
    [Range(0, double.MaxValue)]
    public decimal SoTien { get; set; }

    [Required, Display(Name = "Hình thức")]
    public string HinhThuc { get; set; } = HinhThucThanhToan.TienMat;

    [Required, Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiThanhToan.DaCoc;

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class LichSanViewModel
{
    public DateOnly NgayXem { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<Models.Entities.San> Sans { get; set; } = new();
    public List<Models.Entities.KhungGio> KhungGios { get; set; } = new();
    public Dictionary<(int SanId, int KhungGioId), Models.Entities.DatSan?> ODatSan { get; set; } = new();
}
