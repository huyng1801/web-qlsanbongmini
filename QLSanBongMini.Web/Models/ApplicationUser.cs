using Microsoft.AspNetCore.Identity;

namespace QLSanBongMini.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; } = true;
}
