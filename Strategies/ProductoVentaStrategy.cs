using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.Models;

namespace MiBackend.Strategies
{
    public class ProductoVentaStrategy : IVentaItemStrategy
    {
        private readonly ApplicationDbContext _context;

        public ProductoVentaStrategy(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle)
        {
            var producto = await _context.Productos.FindAsync(detalle.ProductoId)
                ?? throw new KeyNotFoundException($"Producto con ID {detalle.ProductoId} no encontrado");

            if (producto.CantidadLibras < detalle.CantidadLibras)
                throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Nombre}");

            producto.CantidadLibras -= detalle.CantidadLibras;
            
            var ventaDetalle = new VentaDetalle
            {
                Venta = venta,
                TipoItem = "PRODUCTO",
                ProductoId = producto.ProductoId,
                Producto = producto,
                CantidadLibras = detalle.CantidadLibras,
                PrecioUnitario = producto.PrecioPorLibra,
                Subtotal = detalle.CantidadLibras * producto.PrecioPorLibra
            };

            return ventaDetalle;
        }
    }
} 