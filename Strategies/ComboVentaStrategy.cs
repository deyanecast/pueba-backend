using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.Models;

namespace MiBackend.Strategies
{
    public class ComboVentaStrategy : IVentaItemStrategy
    {
        private readonly ApplicationDbContext _context;

        public ComboVentaStrategy(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle)
        {
            if (detalle.ComboId <= 0)
                throw new ArgumentException("ID de combo inválido");

            if (detalle.CantidadLibras <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");

            // Cargar el combo sin tracking
            var combo = await _context.Combos
                .AsNoTracking()
                .Include(c => c.ComboDetalles)
                .FirstOrDefaultAsync(c => c.ComboId == detalle.ComboId && c.EstaActivo)
                ?? throw new KeyNotFoundException($"Combo con ID {detalle.ComboId} no encontrado o no está activo");

            if (!combo.ComboDetalles.Any())
                throw new InvalidOperationException($"El combo {combo.Nombre} no tiene productos asociados");

            // Obtener todos los IDs de productos necesarios
            var productoIds = combo.ComboDetalles.Select(cd => cd.ProductoId).Distinct().ToList();

            // Cargar productos en una sola consulta
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.ProductoId) && p.EstaActivo)
                .ToDictionaryAsync(p => p.ProductoId, p => p);

            // Verificar que todos los productos existen y están activos
            var productosNoEncontrados = productoIds.Where(id => !productos.ContainsKey(id)).ToList();
            if (productosNoEncontrados.Any())
            {
                throw new InvalidOperationException(
                    $"Los siguientes productos no fueron encontrados o no están activos: {string.Join(", ", productosNoEncontrados)}");
            }

            // Calcular cantidades requeridas por producto
            var cantidadesRequeridas = new Dictionary<int, decimal>();
            foreach (var comboDetalle in combo.ComboDetalles)
            {
                var cantidadRequerida = comboDetalle.CantidadLibras * detalle.CantidadLibras;
                if (cantidadesRequeridas.ContainsKey(comboDetalle.ProductoId))
                {
                    cantidadesRequeridas[comboDetalle.ProductoId] += cantidadRequerida;
                }
                else
                {
                    cantidadesRequeridas[comboDetalle.ProductoId] = cantidadRequerida;
                }
            }

            // Verificar stock
            var productosConStockInsuficiente = new List<string>();
            foreach (var (productoId, cantidadRequerida) in cantidadesRequeridas)
            {
                var producto = productos[productoId];
                if (producto.CantidadLibras < cantidadRequerida)
                {
                    productosConStockInsuficiente.Add(
                        $"{producto.Nombre} (Disponible: {producto.CantidadLibras}, Requerido: {cantidadRequerida})");
                }
            }

            if (productosConStockInsuficiente.Any())
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente para los siguientes productos del combo:\n" +
                    string.Join("\n", productosConStockInsuficiente));
            }

            // Actualizar stock
            foreach (var (productoId, cantidadRequerida) in cantidadesRequeridas)
            {
                var producto = productos[productoId];
                producto.CantidadLibras -= cantidadRequerida;
                _context.Update(producto);
            }

            return new VentaDetalle
            {
                Venta = venta,
                TipoItem = "COMBO",
                ComboId = combo.ComboId,
                CantidadLibras = detalle.CantidadLibras,
                PrecioUnitario = combo.Precio,
                Subtotal = detalle.CantidadLibras * combo.Precio
            };
        }
    }
} 