using System.Collections.Generic;
using System.Threading.Tasks;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaResponse>> GetCategoriasAsync();
        Task<CategoriaResponse> GetCategoriaByIdAsync(int id);
        Task<List<CategoriaResponse>> GetActiveCategoriasAsync();
    }
} 