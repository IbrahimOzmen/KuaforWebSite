using System.ComponentModel.DataAnnotations;

namespace KuaforRandevuSistemi.API.DTOs
{
    public class EmployeeResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int SalonId { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public List<ServiceResponseDTO> Services { get; set; } = new();
    }

    public class EmployeeScheduleResponseDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public List<DayScheduleDTO> WeeklySchedule { get; set; } = new();
    }

    public class DayScheduleDTO
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class EmployeeServicesResponseDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public List<ServiceResponseDTO> Services { get; set; } = new();
    }

    public class CreateEmployeeRequestDTO
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter arasında olmalıdır")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        public int SalonId { get; set; }
    }
}