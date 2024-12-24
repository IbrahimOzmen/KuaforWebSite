using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Web.Controllers
{
    [Authorize(Roles = "Customer")] // Sadece Customer rolüne sahip kullanıcılar erişebilir
    public class CustomerController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Müşteri Dashboard'u
        public async Task<IActionResult> Index()
        {
            // Aktif kullanıcının ID'sini al
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Aktif randevuları getir
            var activeAppointments = await _unitOfWork.Appointments
                .FindAsync(a => a.CustomerId == userId &&
                              a.AppointmentStartTime > DateTime.Now &&
                              a.Status != "Cancelled");

            // Geçmiş randevuları getir
            var pastAppointments = await _unitOfWork.Appointments
                .FindAsync(a => a.CustomerId == userId &&
                              a.AppointmentStartTime <= DateTime.Now);

            ViewBag.ActiveAppointments = activeAppointments;
            ViewBag.PastAppointments = pastAppointments;

            return View();
        }

        // Profil sayfası
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // Tüm hizmetleri listele
        public async Task<IActionResult> Services()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return View(services);
        }

        // Tüm çalışanları listele
        public async Task<IActionResult> Employees()
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            return View(employees);
        }

        // Çalışanın çalışma saatlerini görüntüle
        public async Task<IActionResult> EmployeeSchedule(int id)
        {
            var workingHours = await _unitOfWork.WorkingHours
                .FindAsync(w => w.EmployeeId == id);

            if (!workingHours.Any())
                return NotFound();

            return View(workingHours);
        }
    }
}