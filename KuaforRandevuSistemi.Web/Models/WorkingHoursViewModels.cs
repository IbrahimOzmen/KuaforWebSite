// Web/Models/WorkingHoursViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace KuaforRandevuSistemi.Web.Models
{
    public class EmployeeWorkingHoursViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<DayScheduleViewModel> DaySchedules { get; set; }
    }

    public class DayScheduleViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }

        [Required(ErrorMessage = "Başlangıç saati gereklidir")]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati gereklidir")]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }
    }
}