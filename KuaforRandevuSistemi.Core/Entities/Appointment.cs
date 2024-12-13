using KuaforRandevuSistemi.Core.Entities.Common;
using System;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class Appointment : BaseEntity
    {
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }

        public DateTime AppointmentStartTime { get; set; }
        public DateTime AppointmentEndTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; }  // Confirmed, Cancelled, Completed
        public string Notes { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public virtual AppUser Customer { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Service Service { get; set; }
    }

    // Status için enum tanımı
    public enum AppointmentStatus
    {
        Confirmed,  // Randevu alındığında otomatik
        Cancelled,  // Müşteri iptal ederse
        Completed  // Randevu saati geçtiğinde otomatik
    }
}