using KuaforRandevuSistemi.Core.Entities.Common;
using System;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class Appointment : BaseEntity
    {
        private DateTime _appointmentStartTime;
        private DateTime _appointmentEndTime;
        private DateTime _createdAt;

        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }

        public DateTime AppointmentStartTime
        {
            get => _appointmentStartTime;
            set => _appointmentStartTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public DateTime AppointmentEndTime
        {
            get => _appointmentEndTime;
            set => _appointmentEndTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string Status { get; set; } = AppointmentStatus.Pending.ToString();  // Confirmed, Cancelled, Completed
        public string? Notes { get; set; }
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
        Pending,      // Randevu bekliyor
        Confirmed,  // Randevu onaylandı
        Cancelled,  // Randevu iptal edildi
        Completed  // Randevu tamamlandı
    }
}