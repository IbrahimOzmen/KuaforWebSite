using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace KuaforRandevuSistemi.Application.Services
{
    public interface IAuthService
    {
        Task<AppUser?> ValidateUser(string email, string password);
        Task SignInAsync(AppUser user, HttpContext httpContext);
        Task SignOutAsync(HttpContext httpContext);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppUser?> ValidateUser(string email, string password)
        {
            var user = await _unitOfWork.AppUsers
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user != null && user.Password == password) // Gerçek uygulamada hash karşılaştırması yapılmalı
            {
                return user;
            }

            return null;
        }

        public async Task SignInAsync(AppUser user, HttpContext httpContext)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),         
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        public async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}