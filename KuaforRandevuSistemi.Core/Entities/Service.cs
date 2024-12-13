using System;
using System.Collections.Generic;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int SalonId { get; set; }

        public virtual Salon? Salon { get; set; }
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}