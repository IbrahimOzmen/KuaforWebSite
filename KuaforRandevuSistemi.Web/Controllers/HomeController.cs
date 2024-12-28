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
            // Eðer kullanýcý giriþ yapmýþsa
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Admin ise admin paneline yönlendir
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                // Normal kullanýcý ise ana sayfaya devam et
            }

            // Giriþ yapmamýþ kullanýcýlar için ana sayfa
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
