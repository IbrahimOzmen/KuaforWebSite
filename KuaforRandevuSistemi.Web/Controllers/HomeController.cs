using System.Diagnostics;
using KuaforRandevuSistemi.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KuaforRandevuSistemi.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // E�er kullan�c� giri� yapm��sa
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Admin ise admin paneline y�nlendir
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                // Normal kullan�c� ise ana sayfaya devam et
            }

            // Giri� yapmam�� kullan�c�lar i�in ana sayfa
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
