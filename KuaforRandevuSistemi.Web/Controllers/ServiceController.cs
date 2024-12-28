using Microsoft.AspNetCore.Mvc;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;

namespace KuaforRandevuSistemi.Web.Controllers
{
    public class ServiceController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Servisleri Listele
        public async Task<IActionResult> Index()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return View(services);
        }

        // GET: Servis Ekle
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servis Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
            {
                // Hataları görmek için
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ModelState.AddModelError("", $"Doğrulama hataları: {errors}");
                return View(service);
            }

            try
            {
                service.SalonId = 1;
                await _unitOfWork.Services.AddAsync(service);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                return View(service);
            }
        }

        // GET: Servis Düzenle
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Servis Düzenle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingService = await _unitOfWork.Services.GetByIdAsync(id);
                    if (existingService == null)
                    {
                        return NotFound();
                    }

                    existingService.Name = service.Name;
                    existingService.Price = service.Price;
                    existingService.Duration = service.Duration;

                    await _unitOfWork.Services.UpdateAsync(existingService);
                    await _unitOfWork.CompleteAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }
            return View(service);
        }
        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var service = await _unitOfWork.Services.GetByIdAsync(id);
                if (service == null)
                    return NotFound();

                // Önce bu hizmetle ilgili çalışan ilişkilerini sil
                var employeeServices = await _unitOfWork.EmployeeServices
                    .FindAsync(es => es.ServiceId == id);
                foreach (var es in employeeServices)
                {
                    await _unitOfWork.EmployeeServices.DeleteAsync(es);
                }

                // Randevuları kontrol et
                var appointments = await _unitOfWork.Appointments
                    .FindAsync(a => a.ServiceId == id);
                foreach (var appointment in appointments)
                {
                    appointment.Status = "Cancelled";
                    appointment.Notes += " (Hizmet sistemden kaldırıldığı için iptal edildi)";
                    await _unitOfWork.Appointments.UpdateAsync(appointment);
                }

                // Son olarak hizmeti sil
                await _unitOfWork.Services.DeleteAsync(service);
                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = "Hizmet başarıyla silindi.";
                return RedirectToAction("Services", "Admin");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "Hizmet silinirken bir hata oluştu.";
                return RedirectToAction("Services", "Admin");
            }
        }
        public async Task<IActionResult> List()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return View(services); // Views/Service/List.cshtml
        }


    }
}