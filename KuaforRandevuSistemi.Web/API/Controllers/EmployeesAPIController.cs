using Microsoft.AspNetCore.Mvc;
using KuaforRandevuSistemi.API.DTOs;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesAPIController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeesAPIController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/employees
        // Tüm çalışanları ve uzmanlık alanlarını listeler
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDTO>>> GetEmployees()
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            var response = new List<EmployeeResponseDTO>();

            foreach (var employee in employees)
            {
                var employeeServices = await _unitOfWork.EmployeeServices
                    .FindAsync(es => es.EmployeeId == employee.Id);

                var services = new List<ServiceResponseDTO>();
                foreach (var es in employeeServices)
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(es.ServiceId);
                    if (service != null)
                    {
                        services.Add(new ServiceResponseDTO
                        {
                            Id = service.Id,
                            Name = service.Name,
                            Price = service.Price,
                            Duration = service.Duration,
                            SalonId = service.SalonId
                        });
                    }
                }

                var salon = await _unitOfWork.Salons.GetByIdAsync(employee.SalonId);

                response.Add(new EmployeeResponseDTO
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    SalonId = employee.SalonId,
                    SalonName = salon?.Name ?? "Bilinmiyor",
                    Services = services
                });
            }

            return Ok(response);
        }

        // GET: api/employees/{id}/services
        // Belirli bir çalışanın verdiği hizmetleri listeler
        [HttpGet("{id}/services")]
        public async Task<ActionResult<EmployeeServicesResponseDTO>> GetEmployeeServices(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı");

            var employeeServices = await _unitOfWork.EmployeeServices
                .FindAsync(es => es.EmployeeId == id);

            var services = new List<ServiceResponseDTO>();
            foreach (var es in employeeServices)
            {
                var service = await _unitOfWork.Services.GetByIdAsync(es.ServiceId);
                if (service != null)
                {
                    services.Add(new ServiceResponseDTO
                    {
                        Id = service.Id,
                        Name = service.Name,
                        Price = service.Price,
                        Duration = service.Duration,
                        SalonId = service.SalonId
                    });
                }
            }

            return Ok(new EmployeeServicesResponseDTO
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                Services = services
            });
        }

        // GET: api/employees/{id}/schedule
        // Belirli bir çalışanın çalışma programını getirir
        [HttpGet("{id}/schedule")]
        public async Task<ActionResult<EmployeeScheduleResponseDTO>> GetEmployeeSchedule(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı");

            var workingHours = await _unitOfWork.WorkingHours
                .FindAsync(wh => wh.EmployeeId == id);

            var weeklySchedule = new List<DayScheduleDTO>();

            // Haftanın her günü için çalışma saati bilgisini al
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var daySchedule = workingHours.FirstOrDefault(wh => wh.DayOfWeek == day);
                weeklySchedule.Add(new DayScheduleDTO
                {
                    DayOfWeek = day,
                    IsWorkingDay = daySchedule?.IsWorkingDay ?? false,
                    StartTime = daySchedule?.StartTime ?? TimeSpan.Zero,
                    EndTime = daySchedule?.EndTime ?? TimeSpan.Zero
                });
            }

            return Ok(new EmployeeScheduleResponseDTO
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                WeeklySchedule = weeklySchedule
            });
        }
    }
}