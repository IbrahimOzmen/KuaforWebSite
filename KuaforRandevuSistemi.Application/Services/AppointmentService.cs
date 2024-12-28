// Application/Services/AppointmentService.cs
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Application.Interfaces;

namespace KuaforRandevuSistemi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateAppointment(Appointment appointment)
        {
            try
            {
                // Debug için log
                System.Diagnostics.Debug.WriteLine($"Creating appointment: ServiceId={appointment.ServiceId}, EmployeeId={appointment.EmployeeId}");

                // Hizmet kontrolü
                var service = await _unitOfWork.Services.GetByIdAsync(appointment.ServiceId);
                if (service == null)
                {
                    System.Diagnostics.Debug.WriteLine("Service not found");
                    return false;
                }
                appointment.Notes = appointment.Notes ?? string.Empty;
                // Employee kontrolü
                var employee = await _unitOfWork.Employees.GetByIdAsync(appointment.EmployeeId);
                if (employee == null)
                {
                    System.Diagnostics.Debug.WriteLine("Employee not found");
                    return false;
                }

                // Customer kontrolü
                var customer = await _unitOfWork.AppUsers.GetByIdAsync(appointment.CustomerId);
                if (customer == null)
                {
                    System.Diagnostics.Debug.WriteLine("Customer not found");
                    return false;
                }

                appointment.AppointmentStartTime = appointment.AppointmentStartTime;
                appointment.AppointmentEndTime = appointment.AppointmentStartTime.AddMinutes(service.Duration);
                appointment.CreatedAt = DateTime.Now;
                appointment.Status = AppointmentStatus.Pending.ToString(); // Randevu oluşturulurken durumunu "Pending" olarak ayarlayın
                appointment.TotalPrice = service.Price;
                appointment.Notes = appointment.Notes ?? string.Empty;

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.CompleteAsync();
                return true;

                // Debug için yazdır
                System.Diagnostics.Debug.WriteLine($"UTC Times:");
                System.Diagnostics.Debug.WriteLine($"Start: {appointment.AppointmentStartTime:yyyy-MM-dd HH:mm:ss K}");
                System.Diagnostics.Debug.WriteLine($"End: {appointment.AppointmentEndTime:yyyy-MM-dd HH:mm:ss K}");
                System.Diagnostics.Debug.WriteLine($"Created: {appointment.CreatedAt:yyyy-MM-dd HH:mm:ss K}");

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.CompleteAsync();

                System.Diagnostics.Debug.WriteLine("Appointment created successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in CreateAppointment:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> CancelAppointment(int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;

            // Randevu saati geçmişse iptal edilemez
            if (appointment.AppointmentStartTime < DateTime.Now)
                return false;

            return true;
        }

        public async Task<bool> ConfirmCancellation(int appointmentId)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return false;

                appointment.Status = AppointmentStatus.Cancelled.ToString();
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // AppointmentService.cs içinde GetAvailableTimeSlots metodu
        public async Task<List<DateTime>> GetAvailableTimeSlots(int employeeId, int serviceId, DateTime date)
        {
            var availableSlots = new List<DateTime>();

            try
            {
                var workingHours = await _unitOfWork.WorkingHours
                    .FindAsync(w => w.EmployeeId == employeeId && w.DayOfWeek == date.DayOfWeek);

                var workingHour = workingHours.FirstOrDefault();
                if (workingHour == null)
                    return availableSlots;

                var startTime = date.Date.Add(workingHour.StartTime);
                var endTime = date.Date.Add(workingHour.EndTime);

                var currentTime = startTime;
                while (currentTime.AddMinutes(30) <= endTime)
                {
                    availableSlots.Add(currentTime); // UTC dönüşümünü kaldırdık
                    currentTime = currentTime.AddMinutes(30);
                }

                return availableSlots;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAvailableTimeSlots: {ex.Message}");
                return availableSlots;
            }
        }

        public async Task CheckAndCompleteAppointments()
        {
            var activeAppointments = await _unitOfWork.Appointments
                .FindAsync(a => a.Status == AppointmentStatus.Confirmed.ToString() &&
                              a.AppointmentEndTime < DateTime.Now);

            foreach (var appointment in activeAppointments)
            {
                appointment.Status = AppointmentStatus.Completed.ToString();
            }

            await _unitOfWork.CompleteAsync();
        }
        public async Task<List<DateTime>> GetUnavailableTimeSlots(int employeeId, DateTime date)
        {
            try
            {
                var existingAppointments = await _unitOfWork.Appointments
                    .FindAsync(a =>
                        a.EmployeeId == employeeId &&
                        a.AppointmentStartTime.Date == date.Date &&
                        a.Status == "Confirmed"
                    );

                return existingAppointments
                    .Select(a => a.AppointmentStartTime)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<DateTime>();
            }
        }
        public async Task ConfirmAppointment(int appointmentId) // Randevuyu onaylayan metot
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed.ToString();
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task RejectAppointment(int appointmentId) // Randevuyu reddeden metot
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled.ToString();
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}