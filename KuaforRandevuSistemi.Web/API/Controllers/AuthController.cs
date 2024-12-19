using Microsoft.AspNetCore.Mvc;
using KuaforRandevuSistemi.API.DTOs;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        // Constructor: UnitOfWork ve JwtService dependency injection
        public AuthController(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        // Login endpoint'i: Kullanıcı girişi için
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _unitOfWork.AppUsers
                .SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Geçersiz email veya şifre");

            // NOT: Gerçek uygulamada şifre hashlenerek karşılaştırılmalı
            if (user.PasswordHash != request.Password)
                return Unauthorized("Geçersiz email veya şifre");

            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                UserRole = user.Role,
                UserName = user.FullName,
                TokenExpiration = DateTime.UtcNow.AddHours(1)
            });
        }

        // Register endpoint'i: Yeni kullanıcı kaydı için
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _unitOfWork.AppUsers.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Bu email adresi zaten kayıtlı");

            var user = new AppUser
            {
                Email = request.Email,
                PasswordHash = request.Password, // NOT: Gerçek uygulamada hash'lenmeli
                FullName = request.FullName,
                Role = "Customer" // Yeni kayıtlar varsayılan olarak müşteri
            };

            await _unitOfWork.AppUsers.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return Ok();
        }
    }
}