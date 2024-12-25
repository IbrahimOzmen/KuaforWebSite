// KuaforRandevuSistemi.Core/Interfaces/IOpenAIService.cs
using System.Threading.Tasks;

namespace KuaforRandevuSistemi.Core.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> GetHairStyleSuggestions(string imageBase64, string currentHairDescription);
    }
}