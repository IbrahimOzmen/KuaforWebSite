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
                // Hizmet bilgilerini al
                var service = await _unitOfWork.Services.GetByIdAsync(appointment.ServiceId);
                if (service == null)
                    throw new Exception("Seçilen hizmet bulunamadı.");

                // Bitiş zamanını hesapla
                appointment.AppointmentEndTime = appointment.AppointmentStartTime.AddMinutes(service.Duration);
                appointment.Status = AppointmentStatus.Confirmed.ToString();
                appointment.CreatedAt = DateTime.Now;
                appointment.TotalPrice = service.Price;

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
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

        public async Task<List<DateTime>> GetAvailableTimeSlots(int employeeId, int serviceId, DateTime date)
        {
            var availableSlots = new List<DateTime>();

            try
            {
                // Debug için log ekleyelim
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Checking slots for date: {date}, employee: {employeeId}");

                var workingHours = await _unitOfWork.WorkingHours
                    .FindAsync(w => w.EmployeeId == employeeId && w.DayOfWeek == date.DayOfWeek);

                // Çalışma saatlerini kontrol edelim
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {workingHours.Count()} working hour records");

                var workingHour = workingHours.FirstOrDefault();
                if (workingHour == null)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] No working hours found for this day");
                    return availableSlots;
                }

                // Çalışma saatlerini yazdıralım
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Start Time: {workingHour.StartTime}, End Time: {workingHour.EndTime}");

                var startTime = date.Date.Add(workingHour.StartTime);
                var endTime = date.Date.Add(workingHour.EndTime);

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Full start time: {startTime}, Full end time: {endTime}");

                var currentTime = startTime;
                while (currentTime.AddMinutes(30) <= endTime)
                {
                    availableSlots.Add(currentTime);
                    currentTime = currentTime.AddMinutes(30);
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Generated {availableSlots.Count} available slots");
                return availableSlots;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] GetAvailableTimeSlots error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
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
    }
}