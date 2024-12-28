// Web/Controllers/WorkingHoursController.cs
using Microsoft.AspNetCore.Mvc;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Web.Models;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Web.Controllers
{
    public class WorkingHoursController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkingHoursController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Çalışanın çalışma saatlerini görüntüle
        public async Task<IActionResult> EmployeeSchedule(int employeeId)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                return NotFound();

            var workingHours = await _unitOfWork.WorkingHours
                .FindAsync(w => w.EmployeeId == employeeId);

            var viewModel = new EmployeeWorkingHoursViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employee.Name,
                DaySchedules = InitializeWeekSchedule(workingHours.ToList())
            };

            ViewBag.ReturnUrl = Url.Action("Employees", "Admin");
            return View(viewModel);
        }

        // POST: Çalışma saatlerini güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateSchedule(EmployeeWorkingHoursViewModel model)
        {
            // En az bir gün seçili mi kontrolü
            if (!model.DaySchedules.Any(d => d.IsWorkingDay))
            {
                ModelState.AddModelError("", "En az bir çalışma günü seçmelisiniz.");
                return View("EmployeeSchedule", model);
            }

            // Salon bilgilerini al
            var salon = await _unitOfWork.Salons.GetByIdAsync(1); // varsayılan salon ID'si
            if (salon == null)
            {
                ModelState.AddModelError("", "Salon bilgileri bulunamadı.");
                return View("EmployeeSchedule", model);
            }

            // Seçili günlerde saat kontrolü
            foreach (var schedule in model.DaySchedules.Where(d => d.IsWorkingDay))
            {
                // Başlangıç saati bitiş saatinden önce mi?
                if (schedule.StartTime >= schedule.EndTime)
                {
                    ModelState.AddModelError("",
                        $"{schedule.DayOfWeek} günü için başlangıç saati bitiş saatinden önce olmalıdır.");
                    return View("EmployeeSchedule", model);
                }

                // Salon çalışma saatleri içinde mi?
                if (schedule.StartTime < salon.OpeningTime || schedule.EndTime > salon.ClosingTime)
                {
                    ModelState.AddModelError("",
                        $"{schedule.DayOfWeek} günü için çalışma saatleri salon çalışma saatleri içinde olmalıdır. ({salon.OpeningTime.ToString(@"hh\:mm")} - {salon.ClosingTime.ToString(@"hh\:mm")})");
                    return View("EmployeeSchedule", model);
                }
            }
           
            try
            {
                // Mevcut kayıtları sil
                var existingHours = await _unitOfWork.WorkingHours
                    .FindAsync(w => w.EmployeeId == model.EmployeeId);

                foreach (var hour in existingHours)
                {
                    await _unitOfWork.WorkingHours.DeleteAsync(hour);
                }

                // Yeni kayıtları ekle
                foreach (var schedule in model.DaySchedules.Where(d => d.IsWorkingDay))
                {
                    var workingHour = new WorkingHours
                    {
                        EmployeeId = model.EmployeeId,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        IsWorkingDay = true
                    };

                    await _unitOfWork.WorkingHours.AddAsync(workingHour);
                }

                await _unitOfWork.CompleteAsync();
                TempData["Success"] = "Çalışma saatleri başarıyla güncellendi.";
                return RedirectToAction("WorkingHours", "Admin");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Çalışma saatleri güncellenirken bir hata oluştu.";
                return RedirectToAction("WorkingHours", "Admin");
            }

        }

        private List<DayScheduleViewModel> InitializeWeekSchedule(List<WorkingHours> existingHours)
        {
            var schedule = new List<DayScheduleViewModel>();

            for (int i = 0; i < 7; i++)
            {
                var day = (DayOfWeek)i;
                var existingHour = existingHours.FirstOrDefault(h => h.DayOfWeek == day);

                schedule.Add(new DayScheduleViewModel
                {
                    DayOfWeek = day,
                    IsWorkingDay = existingHour?.IsWorkingDay ?? false,
                    StartTime = existingHour?.StartTime ?? new TimeSpan(9, 0, 0),
                    EndTime = existingHour?.EndTime ?? new TimeSpan(17, 0, 0)
                });
            }

            return schedule;
        }
    }
}