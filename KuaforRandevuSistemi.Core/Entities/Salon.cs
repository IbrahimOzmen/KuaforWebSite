using System;
using System.Collections.Generic;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class Salon : BaseEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
}