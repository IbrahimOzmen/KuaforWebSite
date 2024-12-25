// KuaforRandevuSistemi.Web/Controllers/AIStyleController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;
using System;
using System.IO;
using System.Security.Claims;

namespace KuaforRandevuSistemi.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AIStyleController : BaseController
    {
        private readonly IOpenAIService _openAIService;
        private readonly IUnitOfWork _unitOfWork;

        public AIStyleController(IOpenAIService openAIService, IUnitOfWork unitOfWork)
        {
            _openAIService = openAIService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetSuggestion(IFormFile image, string currentHairDescription)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Please upload an image");
                }

                // Convert image to base64
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBase64 = Convert.ToBase64String(ms.ToArray());

                // Get suggestion from OpenAI
                var suggestion = await _openAIService.GetHairStyleSuggestions(imageBase64, currentHairDescription);

                // Save suggestion to database
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var aiSuggestion = new AIStyleSuggestion
                {
                    UserId = userId,
                    ImagePath = $"uploads/{Guid.NewGuid()}{Path.GetExtension(image.FileName)}",
                    Suggestions = suggestion,
                    CreatedAt = DateTime.UtcNow
                };

                // Save image to disk
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", aiSuggestion.ImagePath);
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                await _unitOfWork.AIStyleSuggestions.AddAsync(aiSuggestion);
                await _unitOfWork.CompleteAsync();

                return Json(new { success = true, suggestion = suggestion });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> History()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var suggestions = await _unitOfWork.AIStyleSuggestions
                .FindAsync(s => s.UserId == userId);
            return View(suggestions);
        }
    }
}