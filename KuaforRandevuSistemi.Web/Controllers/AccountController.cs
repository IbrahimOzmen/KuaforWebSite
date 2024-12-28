// Web/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Application.Services;
using KuaforRandevuSistemi.Web.Models;

namespace KuaforRandevuSistemi.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private const string AdminEmail = "g191210010@sakarya.edu.tr";
        private const string AdminPassword = "sau";

        public AccountController(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Zaten giriş yapmış kullanıcıyı yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            // Giriş yapmamış kullanıcı için login view'ı göster
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Admin kontrolü
            if (model.Email == AdminEmail && model.Password == AdminPassword)
            {
                var adminUser = await _unitOfWork.AppUsers
                    .SingleOrDefaultAsync(u => u.Email == AdminEmail);
                if (adminUser != null)
                {
                    await _authService.SignInAsync(adminUser, HttpContext);
                    return RedirectToAction("Index", "Admin");
                }
            }

            // Normal kullanıcı girişi
            var user = await _authService.ValidateUser(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz email veya şifre");
                return View(model);
            }

            await _authService.SignInAsync(user, HttpContext);

            // ReturnUrl varsa oraya yönlendir, yoksa ana sayfaya
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _unitOfWork.AppUsers.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kayıtlı");
                return View(model);
            }

            var user = new AppUser
            {
                Email = model.Email,
                Password = model.Password, // Gerçek uygulamada hash'lenmeli

                Role = "Customer"
            };

            await _unitOfWork.AppUsers.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            await _authService.SignInAsync(user, HttpContext);
            return RedirectToAction("Index", "Home");
        }



        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpGet, HttpPost] // Her iki HTTP metodunu da kabul et
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await _authService.SignOutAsync(HttpContext);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}