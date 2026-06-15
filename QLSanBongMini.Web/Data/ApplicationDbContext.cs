using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Models;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<San> Sans => Set<San>();
    public DbSet<KhungGio> KhungGios => Set<KhungGio>();
    public DbSet<BangGia> BangGias => Set<BangGia>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<DatSan> DatSans => Set<DatSan>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BangGia>()
            .HasIndex(x => new { x.SanId, x.KhungGioId, x.LaCuoiTuan })
            .IsUnique();

        builder.Entity<DatSan>()
            .HasOne(x => x.ThanhToan)
            .WithOne(x => x.DatSan)
            .HasForeignKey<ThanhToan>(x => x.DatSanId);

        builder.Entity<DatSan>()
            .HasOne(x => x.NguoiTao)
            .WithMany()
            .HasForeignKey(x => x.NguoiTaoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
