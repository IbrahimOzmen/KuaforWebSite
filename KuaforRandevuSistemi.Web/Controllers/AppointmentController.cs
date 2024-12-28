using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Application.Interfaces;

[Authorize] // Tüm controller'ı authorize yapalım
public class AppointmentController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IUnitOfWork unitOfWork, IAppointmentService appointmentService)
    {
        _unitOfWork = unitOfWork;
        _appointmentService = appointmentService;
    }

    // Randevu alma akışının ilk adımı - Hizmet seçimi
    public async Task<IActionResult> Create()
    {
        var services = await _unitOfWork.Services.GetAllAsync();
        return View(services);
    }

    // Randevuları listele - Kullanıcı veya admin için
    // AppointmentController.cs
    public async Task<IActionResult> List()
    {
        if (User.IsInRole("Admin"))
        {
            var allAppointments = await _unitOfWork.Appointments.FindWithNavigationPropertiesAsync(a => true);
            return View("AdminList", allAppointments);
        }
        else
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userAppointments = await _unitOfWork.Appointments
                .FindWithNavigationPropertiesAsync(a => a.CustomerId == userId);
            return View("CustomerList", userAppointments);
        }
    }

    // Randevu iptali
    public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        // Admin değilse, sadece kendi randevusunu iptal edebilir
        if (!User.IsInRole("Admin"))
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (appointment.CustomerId != userId)
                return Forbid();
        }

        var canCancel = await _appointmentService.CancelAppointment(id);
        if (!canCancel)
        {
            TempData["Error"] = "Bu randevu iptal edilemez.";
            return RedirectToAction("List");
        }

        await _appointmentService.ConfirmCancellation(id);
        TempData["Success"] = "Randevu başarıyla iptal edildi.";
        return RedirectToAction("List");
    }

    // Hizmet seçildikten sonra çalışan seçimi
    public async Task<IActionResult> SelectEmployee(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null)
            return NotFound();

        // Include ile Employee bilgilerini de çekelim
        var employeeServices = await _unitOfWork.EmployeeServices
            .FindAsync(es => es.ServiceId == serviceId);

        // Employee verilerini ayrıca yükleyelim
        var employees = new List<Employee>();
        foreach (var es in employeeServices)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(es.EmployeeId);
            if (employee != null)
            {
                employees.Add(employee);
            }
        }

        ViewBag.ServiceId = serviceId;
        return View(employees);  // Direkt Employee listesi gönderelim
    }

    // Çalışan seçildikten sonra tarih ve saat seçimi
    public async Task<IActionResult> SelectDateTime(int serviceId, int employeeId)
    {
        ViewBag.ServiceId = serviceId;
        ViewBag.EmployeeId = employeeId;
        return View();
    }

    // Seçilen tarihe göre müsait saatleri getir
    [HttpPost]
    public async Task<IActionResult> GetAvailableSlots(int serviceId, int employeeId, DateTime date)
    {
        try
        {
            var slots = await _appointmentService.GetAvailableTimeSlots(employeeId, serviceId, date);
            return Json(slots);
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error getting available slots: {ex.Message}");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    // Randevuyu kaydet
    public async Task<IActionResult> Confirm(Appointment appointment)
    {
        try
        {
            if (appointment == null)
            {
                System.Diagnostics.Debug.WriteLine("Appointment is null");
                TempData["Error"] = "Randevu bilgileri eksik";
                return RedirectToAction("Create");
            }

            // Daha detaylı debug bilgisi
            System.Diagnostics.Debug.WriteLine($"Appointment Details:");
            System.Diagnostics.Debug.WriteLine($"ServiceId: {appointment.ServiceId}");
            System.Diagnostics.Debug.WriteLine($"EmployeeId: {appointment.EmployeeId}");
            System.Diagnostics.Debug.WriteLine($"StartTime: {appointment.AppointmentStartTime}");
            System.Diagnostics.Debug.WriteLine($"CustomerPhone: {appointment.CustomerPhone}");
            System.Diagnostics.Debug.WriteLine($"Notes: {appointment.Notes}");

            // User ID kontrolü
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                System.Diagnostics.Debug.WriteLine("User ID claim not found");
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı";
                return RedirectToAction("Create");
            }

            var userId = int.Parse(userIdClaim.Value);
            appointment.CustomerId = userId;
            appointment.Status = AppointmentStatus.Confirmed.ToString();
            appointment.CreatedAt = DateTime.Now;

            var result = await _appointmentService.CreateAppointment(appointment);
            if (result)
            {
                TempData["Success"] = "Randevunuz başarıyla oluşturuldu.";
                return RedirectToAction("List");
            }

            System.Diagnostics.Debug.WriteLine("CreateAppointment returned false");
            TempData["Error"] = "Randevu oluşturulurken bir hata oluştu";
            return RedirectToAction("Create");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in Confirm action:");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            TempData["Error"] = $"Randevu oluşturulurken bir hata oluştu: {ex.Message}";
            return RedirectToAction("Create");
        }
    }
    // AppointmentController.cs
    // AppointmentController.cs içinde MyAppointments action'ı
    [Authorize]
    [Authorize]
    public async Task<IActionResult> MyAppointments()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Tek bir sorgu ile ilişkili verileri yükle
            var userAppointments = await _unitOfWork.Appointments
                .FindWithIncludeAsync(
                    a => a.CustomerId == userId,
                    a => a.Service,
                    a => a.Employee
                );

            // Tarihe göre sırala
            var sortedAppointments = userAppointments.OrderByDescending(a => a.AppointmentStartTime);

            return View(sortedAppointments);
        }
        catch (Exception ex)
        {
            // Hata durumunda Error view'ına yönlendir
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<JsonResult> GetUnavailableTimes(int employeeId, DateTime date)
    {
        var unavailableTimes = await _appointmentService.GetUnavailableTimeSlots(employeeId, date);
        return Json(unavailableTimes);
    }

    [HttpPost]
    public async Task<JsonResult> CheckSlotAvailability(int employeeId, DateTime date)
    {
        try
        {
            var existingAppointments = await _unitOfWork.Appointments
                .FindAsync(a =>
                    a.EmployeeId == employeeId &&
                    a.AppointmentStartTime.Date == date.Date &&
                    a.Status == "Confirmed"
                );

            // Sadece dolu olan saatleri döndür
            var bookedTimes = existingAppointments
                .Select(a => a.AppointmentStartTime.ToString("HH:mm"))
                .ToList();

            return Json(bookedTimes);
        }
        catch
        {
            return Json(new List<string>());
        }
    }
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        await _appointmentService.ConfirmAppointment(id);
        return RedirectToAction("List");
    }

    public async Task<IActionResult> RejectAppointment(int id)
    {
        await _appointmentService.RejectAppointment(id);
        return RedirectToAction("List");
    }
}