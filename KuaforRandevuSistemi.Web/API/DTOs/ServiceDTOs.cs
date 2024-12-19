using System.ComponentModel.DataAnnotations;

namespace KuaforRandevuSistemi.API.DTOs
{
    public class ServiceResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }  // Dakika cinsinden
        public int SalonId { get; set; }
    }

    public class CreateServiceDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(5, 360)]  // 5 dakika ile 6 saat arası
        public int Duration { get; set; }

        [Required]
        public int SalonId { get; set; }
    }

    public class UpdateServiceDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(5, 360)]
        public int Duration { get; set; }
    }
}