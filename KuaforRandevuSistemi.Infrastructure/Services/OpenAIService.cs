// KuaforRandevuSistemi.Infrastructure/Services/OpenAIService.cs
using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using KuaforRandevuSistemi.Core.Interfaces;

namespace KuaforRandevuSistemi.Infrastructure.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetHairStyleSuggestions(string imageBase64, string currentHairDescription)
        {
            try
            {
                var messages = new[]
                {
                    new { role = "system", content = "You are a professional hairstylist with expertise in suggesting personalized hairstyles." },
                    new { role = "user", content = $"Based on this hair description: {currentHairDescription}, suggest 3 suitable modern hairstyles that would look good. Consider maintenance requirements, current trends, and daily styling needs. Format your response in three numbered points, with each suggestion including style name, description, and maintenance requirements." }
                };

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",  // GPT-4 modeli kullanıyoruz
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 500
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(API_URL, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"OpenAI API error: {response.StatusCode}, {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug için log
                Console.WriteLine($"OpenAI Response: {responseContent}");

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return jsonResponse
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "Üzgünüm, şu anda öneri oluşturulamıyor.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHairStyleSuggestions: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw new Exception("Saç stili önerileri alınırken bir hata oluştu. Lütfen daha sonra tekrar deneyin.", ex);
            }
        }
    }
}