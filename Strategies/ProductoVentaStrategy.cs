using MiBackend.DTOs.Requests;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Strategies
{
    public class ProductoVentaStrategy : IVentaItemStrategy
    {
        private readonly IProductoService _productoService;

        public ProductoVentaStrategy(IProductoService productoService)
        {
            _productoService = productoService;
        }

        public async Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle)
        {
            // Validar stock
            if (!await _productoService.ValidateProductoStockAsync(detalle.ItemId, detalle.Cantidad))
            {
                throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detalle.ItemId}");
            }

            // Calcular total
            var total = await _productoService.CalculateProductoTotalAsync(detalle.ItemId, detalle.Cantidad);
            var precioUnitario = await _productoService.GetPrecioProductoAsync(detalle.ItemId);

            // Actualizar stock del producto
            await _productoService.UpdateProductoStockAsync(detalle.ItemId, -detalle.Cantidad);

            // Crear detalle de venta
            var ventaDetalle = new VentaDetalle
            {
                VentaId = venta.VentaId,
                TipoItem = detalle.TipoItem,
                ProductoId = detalle.ItemId,
                CantidadLibras = detalle.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = total,
                Venta = venta
            };

            return ventaDetalle;
        }
    }
} 