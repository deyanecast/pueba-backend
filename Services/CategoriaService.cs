using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MiBackend.Models;
using MiBackend.Interfaces;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;

namespace MiBackend.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CategoriaService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private const string ALL_CATEGORIES_CACHE_KEY = "ALL_CATEGORIES";

        public CategoriaService(IMemoryCache cache, ILogger<CategoriaService> logger, IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoriaResponse>> GetCategoriasAsync()
        {
            try
            {
                if (_cache.TryGetValue(ALL_CATEGORIES_CACHE_KEY, out List<CategoriaResponse> cachedCategories))
                {
                    return cachedCategories;
                }

                var categorias = await _unitOfWork.Repository<Categoria>()
                    .Query()
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CategoriaResponse
                    {
                        CategoriaId = c.CategoriaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        EstaActivo = c.EstaActivo
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetSize(1);

                _cache.Set(ALL_CATEGORIES_CACHE_KEY, categorias, cacheEntryOptions);

                return categorias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las categorías");
                throw new InvalidOperationException("Error al obtener categorías. Por favor, inténtelo de nuevo.", ex);
            }
        }

        public async Task<CategoriaResponse> GetCategoriaByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"CATEGORY_{id}";
                if (_cache.TryGetValue(cacheKey, out CategoriaResponse cachedCategory))
                {
                    return cachedCategory;
                }

                var categoria = await _unitOfWork.Repository<Categoria>()
                    .Query()
                    .Where(c => c.CategoriaId == id)
                    .Select(c => new CategoriaResponse
                    {
                        CategoriaId = c.CategoriaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        EstaActivo = c.EstaActivo
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (categoria == null)
                    throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetSize(1);

                _cache.Set(cacheKey, categoria, cacheEntryOptions);

                return categoria;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al obtener categoría por ID {CategoriaId}", id);
                throw new InvalidOperationException($"Error al obtener categoría: {ex.Message}", ex);
            }
        }

        public async Task<List<CategoriaResponse>> GetActiveCategoriasAsync()
        {
            try
            {
                var cacheKey = "ACTIVE_CATEGORIES";
                if (_cache.TryGetValue(cacheKey, out List<CategoriaResponse> cachedCategories))
                {
                    return cachedCategories;
                }

                var categorias = await _unitOfWork.Repository<Categoria>()
                    .Query()
                    .Where(c => c.EstaActivo)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CategoriaResponse
                    {
                        CategoriaId = c.CategoriaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        EstaActivo = c.EstaActivo
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetSize(1);

                _cache.Set(cacheKey, categorias, cacheEntryOptions);

                return categorias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías activas");
                throw new InvalidOperationException("Error al obtener categorías activas. Por favor, inténtelo de nuevo.", ex);
            }
        }

        private void InvalidateCache()
        {
            _cache.Remove(ALL_CATEGORIES_CACHE_KEY);
            _cache.Remove("ACTIVE_CATEGORIES");
        }
    }
} 