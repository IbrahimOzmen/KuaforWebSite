using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Web.Models;

namespace KuaforRandevuSistemi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var todayAppointments = await _unitOfWork.Appointments
                    .FindAsync(a => a.AppointmentStartTime >= today && a.AppointmentStartTime < tomorrow);

                ViewBag.TodayAppointmentCount = todayAppointments.Count();
                ViewBag.EmployeeCount = (await _unitOfWork.Employees.GetAllAsync()).Count();
                ViewBag.ServiceCount = (await _unitOfWork.Services.GetAllAsync()).Count();

                return View();
            }
            catch (Exception)
            {
                ViewBag.TodayAppointmentCount = 0;
                ViewBag.EmployeeCount = 0;
                ViewBag.ServiceCount = 0;
                return View();
            }
        }

        #region Çalışan İşlemleri
        // Çalışan Listesi
        public async Task<IActionResult> Employees()
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            return View(employees);
        }

        // Çalışan Oluşturma
        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Çalışanı ekle
                await _unitOfWork.Employees.AddAsync(employee);
                await _unitOfWork.CompleteAsync();

                // Tüm hizmetleri al ve çalışana ata
                var allServices = await _unitOfWork.Services.GetAllAsync();
                foreach (var service in allServices)
                {
                    var employeeService = new EmployeeService
                    {
                        EmployeeId = employee.Id,
                        ServiceId = service.Id
                    };
                    await _unitOfWork.EmployeeServices.AddAsync(employeeService);
                }

                // Varsayılan çalışma saatlerini ekle
                var salon = await _unitOfWork.Salons.GetByIdAsync(employee.SalonId);
                for (int i = 0; i < 7; i++)
                {
                    var workingHours = new WorkingHours
                    {
                        EmployeeId = employee.Id,
                        DayOfWeek = (DayOfWeek)i,
                        StartTime = salon.OpeningTime,
                        EndTime = salon.ClosingTime,
                        IsWorkingDay = true
                    };
                    await _unitOfWork.WorkingHours.AddAsync(workingHours);
                }

                await _unitOfWork.CommitTransactionAsync();
                TempData["Success"] = "Çalışan başarıyla eklendi.";
                return RedirectToAction("Employees", "Admin");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu: " + ex.Message);
                return View(employee);
            }
        }
        // Çalışan Düzenleme
        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _unitOfWork.Employees.GetByIdAsync(id);
                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    existingEmployee.Name = employee.Name;
                    existingEmployee.Email = employee.Email;

                    await _unitOfWork.Employees.UpdateAsync(existingEmployee);
                    await _unitOfWork.CompleteAsync();

                    TempData["Success"] = "Çalışan başarıyla güncellendi.";
                    return RedirectToAction(nameof(Employees));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }
            return View(employee);
        }

        // Çalışan Silme
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                    return NotFound();

                // Çalışanın çalışma saatlerini sil
                var workingHours = await _unitOfWork.WorkingHours
                    .FindAsync(w => w.EmployeeId == id);
                foreach (var workingHour in workingHours)
                {
                    await _unitOfWork.WorkingHours.DeleteAsync(workingHour);
                }

                // Çalışanın hizmet bağlantılarını sil
                var employeeServices = await _unitOfWork.EmployeeServices
                    .FindAsync(es => es.EmployeeId == id);
                foreach (var employeeService in employeeServices)
                {
                    await _unitOfWork.EmployeeServices.DeleteAsync(employeeService);
                }

                // Çalışana ait randevuları kontrol et
                var appointments = await _unitOfWork.Appointments
                    .FindAsync(a => a.EmployeeId == id);
                foreach (var appointment in appointments)
                {
                    appointment.Status = "Cancelled";
                    appointment.Notes += " (Çalışan sistemden silindiği için iptal edildi)";
                    await _unitOfWork.Appointments.UpdateAsync(appointment);
                }

                await _unitOfWork.Employees.DeleteAsync(employee);
                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = "Çalışan başarıyla silindi.";
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "Çalışan silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            if (!ModelState.IsValid)
                return View(service);

            try
            {
                service.SalonId = 1; // varsayılan salon
                await _unitOfWork.Services.AddAsync(service);
                await _unitOfWork.CompleteAsync();
                TempData["Success"] = "Hizmet başarıyla eklendi.";
                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hizmet eklenirken bir hata oluştu: {ex.Message}");
                return View(service);
            }
        }

        // Çalışan Hizmetleri
        [HttpGet]
        public async Task<IActionResult> EmployeeServices(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound();

            var allServices = await _unitOfWork.Services.GetAllAsync();
            var employeeServices = await _unitOfWork.EmployeeServices
                .FindAsync(es => es.EmployeeId == id);

            var employeeServiceIds = employeeServices.Select(es => es.ServiceId).ToList();

            var viewModel = new EmployeeServicesViewModel
            {
                EmployeeId = id,
                EmployeeName = employee.Name,
                Services = allServices.Select(s => new ServiceCheckBoxViewModel
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    IsSelected = employeeServiceIds.Contains(s.Id)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeServices(int employeeId, List<int> selectedServices)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var currentServices = await _unitOfWork.EmployeeServices
                    .FindAsync(es => es.EmployeeId == employeeId);

                foreach (var service in currentServices)
                {
                    if (!selectedServices.Contains(service.ServiceId))
                    {
                        await _unitOfWork.EmployeeServices.DeleteAsync(service);
                    }
                }

                foreach (var serviceId in selectedServices)
                {
                    if (!currentServices.Any(cs => cs.ServiceId == serviceId))
                    {
                        var employeeService = new EmployeeService
                        {
                            EmployeeId = employeeId,
                            ServiceId = serviceId
                        };
                        await _unitOfWork.EmployeeServices.AddAsync(employeeService);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
                TempData["Success"] = "Çalışan hizmetleri başarıyla güncellendi.";
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "Güncelleme sırasında bir hata oluştu.";
            }

            return RedirectToAction(nameof(Employees));
        }
        #endregion

        #region Randevu
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _unitOfWork.Appointments.GetAllAsync();
            return View(appointments);
        }
        #endregion

        #region Çalışma Saatleri İşlemleri
        // Çalışma saatleri listesi
        public async Task<IActionResult> WorkingHours()
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            return View(employees);
        }

        // Çalışanın çalışma saatlerini düzenleme sayfası
        [HttpGet]
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

            return View(viewModel);
        }

        // Çalışma saatlerini güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateSchedule(EmployeeWorkingHoursViewModel model)
        {
            if (!model.DaySchedules.Any(d => d.IsWorkingDay))
            {
                ModelState.AddModelError("", "En az bir çalışma günü seçmelisiniz.");
                return View("EmployeeSchedule", model);
            }

            var salon = await _unitOfWork.Salons.GetByIdAsync(1); // varsayılan salon ID'si
            if (salon == null)
            {
                ModelState.AddModelError("", "Salon bilgileri bulunamadı.");
                return View("EmployeeSchedule", model);
            }

            foreach (var schedule in model.DaySchedules.Where(d => d.IsWorkingDay))
            {
                if (schedule.StartTime >= schedule.EndTime)
                {
                    ModelState.AddModelError("",
                        $"{schedule.DayOfWeek} günü için başlangıç saati bitiş saatinden önce olmalıdır.");
                    return View("EmployeeSchedule", model);
                }

                if (schedule.StartTime < salon.OpeningTime || schedule.EndTime > salon.ClosingTime)
                {
                    ModelState.AddModelError("",
                        $"{schedule.DayOfWeek} günü için çalışma saatleri salon çalışma saatleri içinde olmalıdır." +
                        $" ({salon.OpeningTime.ToString(@"hh\:mm")} - {salon.ClosingTime.ToString(@"hh\:mm")})");
                    return View("EmployeeSchedule", model);
                }
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Mevcut kayıtları sil
                var existingHours = await _unitOfWork.WorkingHours
                    .FindAsync(w => w.EmployeeId == model.EmployeeId);

                foreach (var hour in existingHours)
                {
                    await _unitOfWork.WorkingHours.DeleteAsync(hour);
                }

                // Yeni kayıtları ekle
                foreach (var schedule in model.DaySchedules)
                {
                    var workingHour = new WorkingHours
                    {
                        EmployeeId = model.EmployeeId,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        IsWorkingDay = schedule.IsWorkingDay
                    };

                    await _unitOfWork.WorkingHours.AddAsync(workingHour);
                }

                await _unitOfWork.CommitTransactionAsync();
                TempData["Success"] = "Çalışma saatleri başarıyla güncellendi.";
                return RedirectToAction(nameof(WorkingHours));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = $"Çalışma saatleri güncellenirken bir hata oluştu: {ex.Message}";
                return View("EmployeeSchedule", model);
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
        #endregion




        #region Hizmet İşlemleri
        public async Task<IActionResult> Services()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return View(services);
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CompleteAsync();
                TempData["Success"] = "Hizmet başarıyla güncellendi.";
                return RedirectToAction("Services");
            }
            return View(service);
        }

        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var service = await _unitOfWork.Services.GetByIdAsync(id);
                if (service == null)
                    return NotFound();

                // İlişkili kayıtları sil/güncelle
                var employeeServices = await _unitOfWork.EmployeeServices
                    .FindAsync(es => es.ServiceId == id);
                foreach (var es in employeeServices)
                {
                    await _unitOfWork.EmployeeServices.DeleteAsync(es);
                }

                var appointments = await _unitOfWork.Appointments
                    .FindAsync(a => a.ServiceId == id);
                foreach (var appointment in appointments)
                {
                    appointment.Status = "Cancelled";
                    appointment.Notes += " (Hizmet kaldırıldığı için iptal edildi)";
                    await _unitOfWork.Appointments.UpdateAsync(appointment);
                }

                await _unitOfWork.Services.DeleteAsync(service);
                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = "Hizmet başarıyla silindi.";
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "Hizmet silinirken bir hata oluştu.";
            }

            return RedirectToAction("Services");
        }
        #endregion
    }
}