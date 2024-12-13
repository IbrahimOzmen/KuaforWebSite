// Core/Entities/WorkingHours.cs
using System;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class WorkingHours : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }  // Enum kullanıyoruz (Pazartesi-Pazar)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorkingDay { get; set; }

        // Navigation property
        public virtual Employee Employee { get; set; }
    }
}