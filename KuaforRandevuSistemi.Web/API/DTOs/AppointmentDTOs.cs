namespace KuaforRandevuSistemi.API.DTOs
{
    public class AvailableSlotDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateAppointmentDTO
    {
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }
        public DateTime AppointmentStartTime { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AppointmentResponseDTO
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public DateTime AppointmentStartTime { get; set; }
        public DateTime AppointmentEndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}