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
            var combo = await _context.Combos
                .Include(c => c.ComboDetalles)
                .ThenInclude(cd => cd.Producto)
                .FirstOrDefaultAsync(c => c.ComboId == detalle.ComboId)
                ?? throw new KeyNotFoundException($"Combo con ID {detalle.ComboId} no encontrado");

            // Verificar stock de todos los productos en el combo
            foreach (var comboDetalle in combo.ComboDetalles)
            {
                var cantidadRequerida = comboDetalle.CantidadLibras * detalle.CantidadLibras;
                if (comboDetalle.Producto.CantidadLibras < cantidadRequerida)
                    throw new InvalidOperationException($"Stock insuficiente para el producto {comboDetalle.Producto.Nombre} en el combo");
                
                comboDetalle.Producto.CantidadLibras -= cantidadRequerida;
            }

            var ventaDetalle = new VentaDetalle
            {
                Venta = venta,
                TipoItem = "COMBO",
                ComboId = combo.ComboId,
                Combo = combo,
                CantidadLibras = detalle.CantidadLibras,
                PrecioUnitario = combo.Precio,
                Subtotal = detalle.CantidadLibras * combo.Precio
            };

            return ventaDetalle;
        }
    }
} 