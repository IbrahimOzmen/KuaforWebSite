using Microsoft.AspNetCore.Mvc;
using KuaforRandevuSistemi.API.DTOs;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServicesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceResponseDTO>>> GetServices()
        {
            var services = await _unitOfWork.Services.GetAllAsync();

            var response = services.Select(s => new ServiceResponseDTO
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Duration = s.Duration,
                SalonId = s.SalonId
            });

            return Ok(response);
        }

        // GET: api/services/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponseDTO>> GetService(int id)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);

            if (service == null)
                return NotFound();

            return Ok(new ServiceResponseDTO
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                Duration = service.Duration,
                SalonId = service.SalonId
            });
        }

        // POST: api/services
        [HttpPost]
        //[Authorize(Roles = "Admin")] // Şimdilik comment
        public async Task<ActionResult<ServiceResponseDTO>> CreateService([FromBody] CreateServiceDTO createServiceDto)
        {
            var service = new Service
            {
                Name = createServiceDto.Name,
                Price = createServiceDto.Price,
                Duration = createServiceDto.Duration,
                SalonId = createServiceDto.SalonId
            };

            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetService),
                new { id = service.Id },
                new ServiceResponseDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                    Price = service.Price,
                    Duration = service.Duration,
                    SalonId = service.SalonId
                });
        }

        // PUT: api/services/{id}
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")] // Şimdilik comment
        public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceDTO updateServiceDto)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);

            if (service == null)
                return NotFound();

            service.Name = updateServiceDto.Name;
            service.Price = updateServiceDto.Price;
            service.Duration = updateServiceDto.Duration;

            await _unitOfWork.Services.UpdateAsync(service);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}