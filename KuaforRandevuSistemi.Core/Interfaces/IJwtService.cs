using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(AppUser user);
        int? ValidateToken(string token);
    }
}