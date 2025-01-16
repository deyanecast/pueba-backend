using MiBackend.DTOs.Requests;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Strategies
{
    public class ComboVentaStrategy : IVentaItemStrategy
    {
        private readonly IComboService _comboService;

        public ComboVentaStrategy(IComboService comboService)
        {
            _comboService = comboService;
        }

        public async Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle)
        {
            // Validar stock
            if (!await _comboService.ValidateComboStockAsync(detalle.ItemId, detalle.Cantidad))
            {
                throw new InvalidOperationException($"Stock insuficiente para el combo con ID {detalle.ItemId}");
            }

            // Calcular total
            var total = await _comboService.CalculateComboTotalAsync(detalle.ItemId, detalle.Cantidad);

            // Crear detalle de venta
            var ventaDetalle = new VentaDetalle
            {
                VentaId = venta.VentaId,
                TipoItem = detalle.TipoItem,
                ComboId = detalle.ItemId,
                CantidadLibras = detalle.Cantidad,
                PrecioUnitario = await _comboService.GetPrecioComboAsync(detalle.ItemId),
                Subtotal = total,
                Venta = venta
            };

            return ventaDetalle;
        }
    }
} 