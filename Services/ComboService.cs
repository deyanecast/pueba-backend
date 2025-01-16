using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Models;
using Microsoft.Extensions.Logging;

namespace MiBackend.Services
{
    public class ComboService : IComboService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductoService _productoService;
        private readonly ILogger<ComboService> _logger;

        public ComboService(
            IUnitOfWork unitOfWork,
            IProductoService productoService,
            ILogger<ComboService> logger)
        {
            _unitOfWork = unitOfWork;
            _productoService = productoService;
            _logger = logger;
        }

        public async Task<ComboResponse> CreateComboAsync(CreateComboRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Verificar productos y cargarlos en memoria
                var productosIds = request.Productos.Select(p => p.ProductoId).Distinct().ToList();
                var productos = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => productosIds.Contains(p.ProductoId))
                    .ToDictionaryAsync(p => p.ProductoId, p => p);

                // Verificar que todos los productos existan
                var productosNoEncontrados = productosIds
                    .Where(id => !productos.ContainsKey(id))
                    .ToList();

                if (productosNoEncontrados.Any())
                {
                    throw new KeyNotFoundException(
                        $"Los siguientes productos no fueron encontrados: {string.Join(", ", productosNoEncontrados)}");
                }

                // Crear el combo
                var combo = new Combo
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Precio = request.Precio,
                    EstaActivo = true,
                    UltimaActualizacion = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Combo>().AddAsync(combo);
                await _unitOfWork.SaveChangesAsync();

                // Crear los detalles del combo
                foreach (var detalle in request.Productos)
                {
                    var comboDetalle = new ComboDetalle
                    {
                        ComboId = combo.ComboId,
                        ProductoId = detalle.ProductoId,
                        CantidadLibras = detalle.CantidadLibras
                    };
                    await _unitOfWork.Repository<ComboDetalle>().AddAsync(comboDetalle);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Cargar el combo completo
                return await GetComboByIdAsync(combo.ComboId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear combo");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<ComboResponse>> GetCombosAsync()
        {
            var combos = await _unitOfWork.Repository<Combo>()
                .Query()
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .OrderBy(c => c.Nombre)
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
                        {
                            ComboDetalleId = cd.ComboDetalleId,
                            Producto = new ProductoResponse
                            {
                                ProductoId = cd.Producto!.ProductoId,
                                Nombre = cd.Producto.Nombre,
                                CantidadLibras = cd.Producto.CantidadLibras,
                                PrecioPorLibra = cd.Producto.PrecioPorLibra,
                                TipoEmpaque = cd.Producto.TipoEmpaque,
                                EstaActivo = cd.Producto.EstaActivo,
                                UltimaActualizacion = cd.Producto.UltimaActualizacion
                            },
                            CantidadLibras = cd.CantidadLibras
                        }).ToList()
                })
                .ToListAsync();

            return combos;
        }

        public async Task<ComboResponse> GetComboByIdAsync(int id)
        {
            var combo = await _unitOfWork.Repository<Combo>()
                .Query()
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .Where(c => c.ComboId == id)
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
                        {
                            ComboDetalleId = cd.ComboDetalleId,
                            Producto = new ProductoResponse
                            {
                                ProductoId = cd.Producto!.ProductoId,
                                Nombre = cd.Producto.Nombre,
                                CantidadLibras = cd.Producto.CantidadLibras,
                                PrecioPorLibra = cd.Producto.PrecioPorLibra,
                                TipoEmpaque = cd.Producto.TipoEmpaque,
                                EstaActivo = cd.Producto.EstaActivo,
                                UltimaActualizacion = cd.Producto.UltimaActualizacion
                            },
                            CantidadLibras = cd.CantidadLibras
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {id} no encontrado");

            return combo;
        }

        public async Task<List<ComboResponse>> GetActiveCombosAsync()
        {
            var combos = await _unitOfWork.Repository<Combo>()
                .Query()
                .Where(c => c.EstaActivo)
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .OrderBy(c => c.Nombre)
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
                        {
                            ComboDetalleId = cd.ComboDetalleId,
                            Producto = new ProductoResponse
                            {
                                ProductoId = cd.Producto!.ProductoId,
                                Nombre = cd.Producto.Nombre,
                                CantidadLibras = cd.Producto.CantidadLibras,
                                PrecioPorLibra = cd.Producto.PrecioPorLibra,
                                TipoEmpaque = cd.Producto.TipoEmpaque,
                                EstaActivo = cd.Producto.EstaActivo,
                                UltimaActualizacion = cd.Producto.UltimaActualizacion
                            },
                            CantidadLibras = cd.CantidadLibras
                        }).ToList()
                })
                .ToListAsync();

            return combos;
        }

        public async Task<bool> UpdateComboStatusAsync(int id, bool isActive)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var combo = await _unitOfWork.Repository<Combo>().GetByIdAsync(id);
                if (combo == null)
                    throw new KeyNotFoundException($"Combo con ID {id} no encontrado");

                combo.EstaActivo = isActive;
                combo.UltimaActualizacion = DateTime.UtcNow;

                _unitOfWork.Repository<Combo>().Update(combo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del combo");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ValidateComboStockAsync(int comboId, decimal cantidad)
        {
            var comboDetalles = await _unitOfWork.Repository<ComboDetalle>()
                .Query()
                .Where(cd => cd.ComboId == comboId)
                .ToListAsync();

            foreach (var detalle in comboDetalles)
            {
                if (!await _productoService.ValidateProductoStockAsync(
                    detalle.ProductoId,
                    detalle.CantidadLibras * cantidad))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<decimal> CalculateComboTotalAsync(int comboId, decimal cantidad)
        {
            var combo = await _unitOfWork.Repository<Combo>()
                .Query()
                .FirstOrDefaultAsync(c => c.ComboId == comboId);

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {comboId} no encontrado");

            if (!combo.EstaActivo)
                throw new InvalidOperationException($"El combo no está activo");

            return combo.Precio * cantidad;
        }

        public async Task<decimal> GetPrecioComboAsync(int comboId)
        {
            var combo = await _unitOfWork.Repository<Combo>()
                .Query()
                .Where(c => c.ComboId == comboId)
                .Select(c => new { c.Precio })
                .FirstOrDefaultAsync();

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {comboId} no encontrado");

            return combo.Precio;
        }
    }
} 