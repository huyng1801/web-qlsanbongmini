using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedBusinessDataAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [AppRoles.Admin, AppRoles.NhanVien];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await CreateUserIfNotExistsAsync(userManager, "admin", "Admin@123", "Quản trị viên", AppRoles.Admin);
        await CreateUserIfNotExistsAsync(userManager, "nhanvien", "Nhanvien@123", "Nhân viên lễ tân", AppRoles.NhanVien);
    }

    private static async Task CreateUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string password,
        string hoTen,
        string role)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = $"{userName}@qlsanbong.local",
                EmailConfirmed = true,
                HoTen = hoTen
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return;
        }

        // Đảm bảo user demo luôn đăng nhập được sau deploy
        if (userName is "admin" or "nhanvien")
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, password);
            user.DangHoatDong = true;
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedBusinessDataAsync(ApplicationDbContext context)
    {
        if (await context.Sans.AnyAsync())
        {
            return;
        }

        var khungGios = new List<KhungGio>
        {
            new() { TenKhung = "Sáng (6h-12h)", GioBatDau = new TimeSpan(6, 0, 0), GioKetThuc = new TimeSpan(12, 0, 0) },
            new() { TenKhung = "Chiều (12h-18h)", GioBatDau = new TimeSpan(12, 0, 0), GioKetThuc = new TimeSpan(18, 0, 0) },
            new() { TenKhung = "Tối (18h-23h)", GioBatDau = new TimeSpan(18, 0, 0), GioKetThuc = new TimeSpan(23, 0, 0) }
        };
        context.KhungGios.AddRange(khungGios);

        var sans = new List<San>
        {
            new() { TenSan = "Sân A - 5 người", LoaiSan = "5v5", MoTa = "Sân cỏ nhân tạo, có đèn chiếu sáng" },
            new() { TenSan = "Sân B - 5 người", LoaiSan = "5v5", MoTa = "Sân cỏ nhân tạo tiêu chuẩn" },
            new() { TenSan = "Sân C - 7 người", LoaiSan = "7v7", MoTa = "Sân rộng, phù hợp đá phủi" },
            new() { TenSan = "Sân D - 5 người", LoaiSan = "5v5", MoTa = "Sân mới, mặt sân êm" }
        };
        context.Sans.AddRange(sans);
        await context.SaveChangesAsync();

        var bangGias = new List<BangGia>();
        foreach (var san in sans)
        {
            foreach (var khung in khungGios)
            {
                var giaNgayThuong = khung.TenKhung.StartsWith("Tối") ? 350_000m :
                    khung.TenKhung.StartsWith("Chiều") ? 280_000m : 200_000m;
                var giaCuoiTuan = giaNgayThuong + 50_000m;

                bangGias.Add(new BangGia { SanId = san.Id, KhungGioId = khung.Id, Gia = giaNgayThuong, LaCuoiTuan = false });
                bangGias.Add(new BangGia { SanId = san.Id, KhungGioId = khung.Id, Gia = giaCuoiTuan, LaCuoiTuan = true });
            }
        }
        context.BangGias.AddRange(bangGias);

        var khachHangs = new List<KhachHang>
        {
            new() { HoTen = "Nguyễn Văn An", SoDienThoai = "0901234567" },
            new() { HoTen = "Trần Thị Bình", SoDienThoai = "0912345678" },
            new() { HoTen = "Lê Minh Cường", SoDienThoai = "0923456789" }
        };
        context.KhachHangs.AddRange(khachHangs);
        await context.SaveChangesAsync();

        var homNay = DateOnly.FromDateTime(DateTime.Today);
        var datSans = new List<DatSan>
        {
            new()
            {
                SanId = sans[0].Id,
                KhachHangId = khachHangs[0].Id,
                NgayDat = homNay,
                GioBatDau = new TimeSpan(18, 0, 0),
                GioKetThuc = new TimeSpan(19, 30, 0),
                TrangThai = TrangThaiDatSan.DaXacNhan,
                TongTien = 350_000m,
                GhiChu = "Đặt cố định thứ 2, 4"
            },
            new()
            {
                SanId = sans[1].Id,
                KhachHangId = khachHangs[1].Id,
                NgayDat = homNay.AddDays(1),
                GioBatDau = new TimeSpan(6, 0, 0),
                GioKetThuc = new TimeSpan(7, 30, 0),
                TrangThai = TrangThaiDatSan.DaThanhToan,
                TongTien = 200_000m
            }
        };
        context.DatSans.AddRange(datSans);
        await context.SaveChangesAsync();

        context.ThanhToans.Add(new ThanhToan
        {
            DatSanId = datSans[1].Id,
            SoTien = 200_000m,
            HinhThuc = HinhThucThanhToan.ChuyenKhoan,
            TrangThai = TrangThaiThanhToan.DaThanhToanDu,
            NgayThanhToan = DateTime.Now
        });
        await context.SaveChangesAsync();
    }
}
