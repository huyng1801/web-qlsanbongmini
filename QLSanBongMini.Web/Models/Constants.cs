namespace QLSanBongMini.Web.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string NhanVien = "NhanVien";
}

public static class TrangThaiSan
{
    public const string HoatDong = "HoatDong";
    public const string BaoTri = "BaoTri";

    public static readonly Dictionary<string, string> Labels = new()
    {
        [HoatDong] = "Hoạt động",
        [BaoTri] = "Bảo trì"
    };
}

public static class TrangThaiDatSan
{
    public const string ChoXacNhan = "ChoXacNhan";
    public const string DaXacNhan = "DaXacNhan";
    public const string DaThanhToan = "DaThanhToan";
    public const string DaHuy = "DaHuy";

    public static readonly Dictionary<string, string> Labels = new()
    {
        [ChoXacNhan] = "Chờ xác nhận",
        [DaXacNhan] = "Đã xác nhận",
        [DaThanhToan] = "Đã thanh toán",
        [DaHuy] = "Đã hủy"
    };
}

public static class HinhThucThanhToan
{
    public const string TienMat = "TienMat";
    public const string ChuyenKhoan = "ChuyenKhoan";

    public static readonly Dictionary<string, string> Labels = new()
    {
        [TienMat] = "Tiền mặt",
        [ChuyenKhoan] = "Chuyển khoản"
    };
}

public static class TrangThaiThanhToan
{
    public const string ChuaThanhToan = "ChuaThanhToan";
    public const string DaCoc = "DaCoc";
    public const string DaThanhToanDu = "DaThanhToanDu";

    public static readonly Dictionary<string, string> Labels = new()
    {
        [ChuaThanhToan] = "Chưa thanh toán",
        [DaCoc] = "Đã cọc",
        [DaThanhToanDu] = "Đã thanh toán đủ"
    };
}
