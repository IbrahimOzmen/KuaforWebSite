using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KuaforRandevuSistemi.API.DTOs;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Application.Interfaces;
using System.Security.Claims;

namespace KuaforRandevuSistemi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(
            IUnitOfWork unitOfWork,
            IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _appointmentService = appointmentService;
        }

        // Müsait randevu saatlerini getir
        [HttpGet("available-slots")]
        public async Task<ActionResult<IEnumerable<AvailableSlotDTO>>> GetAvailableSlots(
            [FromQuery] int employeeId,
            [FromQuery] int serviceId,
            [FromQuery] DateTime date)
        {
            var slots = await _appointmentService.GetAvailableTimeSlots(employeeId, serviceId, date);

            var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
            if (service == null)
                return NotFound("Hizmet bulunamadı");

            var availableSlots = slots.Select(slot => new AvailableSlotDTO
            {
                StartTime = slot,
                EndTime = slot.AddMinutes(service.Duration),
                IsAvailable = true
            });

            return Ok(availableSlots);
        }

        // Randevu oluştur
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDTO>> CreateAppointment(
            [FromBody] CreateAppointmentDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var appointment = new Appointment
            {
                CustomerId = userId,
                EmployeeId = request.EmployeeId,
                ServiceId = request.ServiceId,
                AppointmentStartTime = request.AppointmentStartTime,
                CustomerPhone = request.CustomerPhone,
                Notes = request.Notes ?? string.Empty,  // Eğer null ise boş string atar
            };

            var result = await _appointmentService.CreateAppointment(appointment);
            if (!result)
                return BadRequest("Randevu oluşturulamadı");

            var response = await GetAppointmentResponse(appointment.Id);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, response);
        }

        // Randevu detayı getir
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentResponseDTO>> GetAppointmentById(int id)
        {
            var response = await GetAppointmentResponse(id);
            if (response == null)
                return NotFound();

            // Müşteri sadece kendi randevularını görebilir
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment?.CustomerId != userId)
                    return Forbid();
            }

            return Ok(response);
        }

        // Tüm randevuları listele (Admin için)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDTO>>> GetAllAppointments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            var appointments = await _unitOfWork.Appointments.GetAllAsync();

            if (startDate.HasValue)
                appointments = appointments.Where(a => a.AppointmentStartTime >= startDate.Value);

            if (endDate.HasValue)
                appointments = appointments.Where(a => a.AppointmentStartTime <= endDate.Value);

            if (!string.IsNullOrEmpty(status))
                appointments = appointments.Where(a => a.Status == status);

            var responses = await Task.WhenAll(
                appointments.Select(async a => await GetAppointmentResponse(a.Id)));

            return Ok(responses.Where(r => r != null));
        }

        // Müşterinin kendi randevularını listele
        [Authorize(Roles = "Customer")]
        [HttpGet("my-appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDTO>>> GetMyAppointments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var appointments = await _unitOfWork.Appointments
                .FindAsync(a => a.CustomerId == userId);

            var responses = await Task.WhenAll(
                appointments.Select(async a => await GetAppointmentResponse(a.Id)));

            return Ok(responses.Where(r => r != null));
        }

        // Randevu iptal et
        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelAppointment(int id)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();

            // Müşteri sadece kendi randevusunu iptal edebilir
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                if (appointment.CustomerId != userId)
                    return Forbid();
            }

            var canCancel = await _appointmentService.CancelAppointment(id);
            if (!canCancel)
                return BadRequest("Bu randevu iptal edilemez");

            var success = await _appointmentService.ConfirmCancellation(id);
            if (!success)
                return BadRequest("Randevu iptali başarısız oldu");

            return Ok();
        }

        private async Task<AppointmentResponseDTO?> GetAppointmentResponse(int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;

            var customer = await _unitOfWork.AppUsers.GetByIdAsync(appointment.CustomerId);
            var employee = await _unitOfWork.Employees.GetByIdAsync(appointment.EmployeeId);
            var service = await _unitOfWork.Services.GetByIdAsync(appointment.ServiceId);

            return new AppointmentResponseDTO
            {
                Id = appointment.Id,
                CustomerName = customer?.FullName ?? "",
                EmployeeName = employee?.Name ?? "",
                ServiceName = service?.Name ?? "",
                ServicePrice = service?.Price ?? 0,
                AppointmentStartTime = appointment.AppointmentStartTime,
                AppointmentEndTime = appointment.AppointmentEndTime,
                Status = appointment.Status,
                CustomerPhone = appointment.CustomerPhone,
                Notes = appointment.Notes
            };
        }
    }
}