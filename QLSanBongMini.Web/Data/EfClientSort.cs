using Microsoft.EntityFrameworkCore;
using QLSanBongMini.Web.Models.Entities;

namespace QLSanBongMini.Web.Data;

/// <summary>
/// SQLite không dịch được ORDER BY trên TimeSpan — sắp xếp phía client.
/// </summary>
internal static class EfClientSort
{
    public static async Task<List<KhungGio>> KhungGioByGioBatDauAsync(this IQueryable<KhungGio> query, CancellationToken ct = default)
    {
        var items = await query.ToListAsync(ct);
        return items.OrderBy(x => x.GioBatDau).ToList();
    }

    public static List<DatSan> DatSanByNgayGioDesc(this IEnumerable<DatSan> items) =>
        items.OrderByDescending(x => x.NgayDat).ThenByDescending(x => x.GioBatDau).ToList();

    public static List<DatSan> DatSanByNgayGioAsc(this IEnumerable<DatSan> items) =>
        items.OrderByDescending(x => x.NgayDat).ThenBy(x => x.GioBatDau).ToList();
}
