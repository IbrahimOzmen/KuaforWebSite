using System.Collections.Generic;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int SalonId { get; set; }

        // Navigation property
        public virtual Salon? Salon { get; set; }

        // Koleksiyonları boş liste olarak başlat
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
        public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}