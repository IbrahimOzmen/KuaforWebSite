// Infrastructure/Data/SeedData.cs
using KuaforRandevuSistemi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KuaforRandevuSistemi.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Admin kullanıcısı
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 1,
                    Email = "g211210031@sakarya.edu.tr",
                    Password = "sau",  // Gerçek uygulamada hash'lenmiş olmalı
                    Role = "Admin",
                    
                }
            );

            // Örnek salon
            modelBuilder.Entity<Salon>().HasData(
                new Salon
                {
                    Id = 1,
                    Name = "Ana Salon",
                    Address = "Sakarya",
                    Phone = "1234567890",
                    OpeningTime = new TimeSpan(9, 0, 0),  // 09:00
                    ClosingTime = new TimeSpan(18, 0, 0), // 18:00
                    IsActive = true
                }
            );
        }
    }
}