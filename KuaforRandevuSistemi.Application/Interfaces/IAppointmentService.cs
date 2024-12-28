using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<bool> CreateAppointment(Appointment appointment);
        Task<bool> CancelAppointment(int appointmentId);
        Task<bool> ConfirmCancellation(int appointmentId);
        Task CheckAndCompleteAppointments();
        Task<List<DateTime>> GetAvailableTimeSlots(int employeeId, int serviceId, DateTime date);

        Task<List<DateTime>> GetUnavailableTimeSlots(int employeeId, DateTime date);

        Task ConfirmAppointment(int appointmentId); // ConfirmAppointment metotunu ekleyin
        Task RejectAppointment(int appointmentId); // RejectAppointment metotunu ekleyin
    }
}