using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSanBongMini.Web.Services;

namespace QLSanBongMini.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IBaoCaoService _baoCaoService;

    public DashboardController(IBaoCaoService baoCaoService)
    {
        _baoCaoService = baoCaoService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _baoCaoService.LayThongKeAsync();
        return View(model);
    }
}
